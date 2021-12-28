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
        private readonly Object pMutex = new Object();
        private readonly Object wMutex = new Object();

        public ConcurrentTaskBuffer() : base()
        {
            pSem = new Semaphore(1, 1);
            wSem = new Semaphore(0, 1);
        }

        /// <summary>
        /// Adds the given task to the queue. The implementation must support concurrent accesses.
        /// </summary>
        /// <param name="task">A task to wait in the queue for the execution</param>
        public override void AddTask(TaskDecryption task)
        {
            //pSem.WaitOne();
            lock(pMutex) {
                base.taskBuffer.Enqueue(task);
                Console.WriteLine("enqueued taskId={0}", task.id);
            
                base.numOfTasks++;
                base.maxBuffSize = base.taskBuffer.Count > base.maxBuffSize ? base.taskBuffer.Count  : base.maxBuffSize;

                base.LogVisualisation();
                this.PrintBufferSize();
            }
            //wSem.Release();
        }

        /// <summary>
        /// Picks the next task to be executed. The implementation must support concurrent accesses.
        /// </summary>
        /// <returns>Next task from the list to be executed. Null if there is no task.</returns>
        public override TaskDecryption GetNextTask()
        {
            //todo: implement this method such that satisfies a thread safe shared buffer.
            TaskDecryption t = null;
            
            //wSem.WaitOne();
            lock(wMutex) {
                if (base.taskBuffer.Count > 0)
                {
                    
                    t = base.taskBuffer.Dequeue();
                    if (t.id < 0) {
                        base.taskBuffer.Enqueue(t);
                    }
                }
            }
            //pSem.Release();

            return t;
        }

        /// <summary>
        /// Prints the number of elements available in the buffer.
        /// </summary>
        public override void PrintBufferSize()
        {
            //todo: implement this method such that satisfies a thread safe shared buffer.
            Console.Write("Buffer#{0} ; ", this.taskBuffer.Count);
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

            for (int i = 0; i < providers.Length; i++) {
                providers[i] = new Provider(tasks, base.challenges);
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
                Thread.Sleep(50);
            }

            foreach(Thread t in threads) {
                t.Join();
            }

            Console.WriteLine("thread test; everything is joined i hope");

            return tasks.GetLogs();
        }
    }
}
