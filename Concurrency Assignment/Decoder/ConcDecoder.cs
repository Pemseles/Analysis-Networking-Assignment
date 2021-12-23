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
            pSem.WaitOne();
            base.taskBuffer.Enqueue(task);
            wSem.Release();
            base.numOfTasks++;
            base.maxBuffSize = base.taskBuffer.Count > base.maxBuffSize ? base.taskBuffer.Count  : base.maxBuffSize;
        }

        /// <summary>
        /// Picks the next task to be executed. The implementation must support concurrent accesses.
        /// </summary>
        /// <returns>Next task from the list to be executed. Null if there is no task.</returns>
        public override TaskDecryption GetNextTask()
        {
            //todo: implement this method such that satisfies a thread safe shared buffer.
            TaskDecryption t = null;
            
            return t;
        }

        /// <summary>
        /// Prints the number of elements available in the buffer.
        /// </summary>
        public override void PrintBufferSize()
        {
            //todo: implement this method such that satisfies a thread safe shared buffer.
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
            Provider[] providers = new Provider[FixedParams.numOfProviders];
            Worker[] workers = new Worker[WorkingParams.numOfWorkers];
            Thread[] pThreads = new Thread[FixedParams.numOfProviders];
            Thread[] wThreads = new Thread[WorkingParams.numOfWorkers];

            for (int i = 0; i < providers.Length; i++) {
                providers[i] = new Provider(tasks, base.challenges);
            }
            for (int i = 0; i < workers.Length; i++) {
                workers[i] = new Worker(tasks);
            }

            for (int i = 0; i < pThreads.Length; i++) {
                for (int j = 0; j < base.challenges.Length; j++) {
                    pThreads[i] = new Thread(() => tasks.AddTask(new TaskDecryption(j, base.challenges[j])));
                }
                pThreads[i + 1] = new Thread(() => tasks.AddTask(new TaskDecryption(FixedParams.terminatingTaskId, "")));
            }
            for (int i = 0; i < wThreads.Length; i++) {
                wThreads[i] = new Thread(workers[i].ExecuteTasks);
            }

            foreach (Thread p in pThreads) {
                p.Start();
            }
            foreach (Thread w in wThreads) {
                w.Start();
            }

            foreach (Thread p in pThreads) {
                p.Join();
            }
            foreach (Thread w in wThreads) {
                w.Join();
            }

            Console.WriteLine("thread test a aaa ");

            return tasks.GetLogs();
        }
    }
}
