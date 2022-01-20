using System;
using Decoder;
using System.Threading;

namespace ConcDecoder
{
    /// <summary>
    /// A concurrent version of the class Buffer
    /// Note: For the final solution this class MUST be implemented.
    /// </summary>
    public class ConcurrentTaskBuffer : TaskBuffer
    {
        // pSem = Provider Semaphore & wSem = Worker Semaphore
        public Semaphore pSem, wSem;
        private readonly Object pMutex;
        private readonly Object wMutex;

        public ConcurrentTaskBuffer() : base()
        {
            pMutex = new Object();
            wMutex = new Object();
        }

        /// <summary>
        /// Adds the given task to the queue. The implementation must support concurrent accesses.
        /// </summary>
        /// <param name="task">A task to wait in the queue for the execution</param>
        public override void AddTask(TaskDecryption task)
        {
            lock(pMutex) {
                lock (wMutex) {
                    base.taskBuffer.Enqueue(task);
            
                    base.numOfTasks++;
                    base.maxBuffSize = base.taskBuffer.Count > base.maxBuffSize ? base.taskBuffer.Count  : base.maxBuffSize;

                    base.LogVisualisation();
                    this.PrintBufferSize();
                }
            }
        }

        /// <summary>
        /// Picks the next task to be executed. The implementation must support concurrent accesses.
        /// </summary>
        /// <returns>Next task from the list to be executed. Null if there is no task.</returns>
        public override TaskDecryption GetNextTask()
        {
            TaskDecryption t = null;
            
            lock(wMutex) {
                if (base.taskBuffer.Count > 0)
                {
                    t = base.taskBuffer.Dequeue();
                    if (t.id < 0) {
                        base.taskBuffer.Enqueue(t);
                    }
                }
            }

            return t;
        }

        /// <summary>
        /// Prints the number of elements available in the buffer.
        /// </summary>
        public override void PrintBufferSize()
        {
            Console.Write("Buffer#{0} ; ", taskBuffer.Count);
        }
    }

    class ConcLaunch : Launch
    {
        public ConcLaunch() : base(){  }

        /// <summary>
        /// This method implements the concurrent version of the decryption of provided challenges.
        /// </summary>
        /// <param name="numOfProviders">Number of providers</param>
        /// <param name="numOfWorkers">Number of workers</param>
        /// <returns>Information logged during the execution.</returns>
        public string ConcurrentTaskExecution(int numOfProviders, int numOfWorkers)
        {
            ConcurrentTaskBuffer tasks = new ConcurrentTaskBuffer();
            Provider[] providers = new Provider[numOfProviders];
            Worker[] workers = new Worker[numOfWorkers];
            System.Collections.Generic.LinkedList<Thread> threads = new System.Collections.Generic.LinkedList<Thread>();

            if (numOfProviders > 1) {
                string[][] dividedChal = new string[numOfProviders][];
                double avgLen = (double)base.challenges.Length / numOfProviders;

                double processedLen = 0.0;
                int currentStart = 0;
                int currentEnd = 0;
                int partLen = 0;

                for (int i = 0; i < numOfProviders; i++) {
                    processedLen += avgLen;
                    currentEnd = (int)Math.Round(processedLen);
                    partLen = currentEnd - currentStart;
                    dividedChal[i] = new string[partLen];
                    
                    Array.Copy(base.challenges, currentStart, dividedChal[i], 0, partLen);

                    currentStart = currentEnd;
                    providers[i] = new Provider(tasks, dividedChal[i]);
                }
            }
            else {
                for (int i = 0; i < providers.Length; i++) {
                    providers[i] = new Provider(tasks, base.challenges);
                }
            }
            for (int i = 0; i < workers.Length; i++) {
                workers[i] = new Worker(tasks);
            }

            for (int i = 0; i < workers.Length; i++) {
                threads.AddFirst(new Thread(workers[i].ExecuteTasks));
            }
            for (int i = 0; i < providers.Length; i++) {
                threads.AddFirst(new Thread(providers[i].SendTasks));
            }

            foreach(Thread t in threads) {
                t.Start();
            }

            foreach(Thread t in threads) {
                t.Join();
            }

            return tasks.GetLogs();
        }
    }
}
