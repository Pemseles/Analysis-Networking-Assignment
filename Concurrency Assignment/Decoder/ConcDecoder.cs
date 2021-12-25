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
            
            base.numOfTasks++;
            base.maxBuffSize = base.taskBuffer.Count > base.maxBuffSize ? base.taskBuffer.Count  : base.maxBuffSize;

            base.LogVisualisation();
            base.PrintBufferSize();

            wSem.Release();
        }

        /// <summary>
        /// Picks the next task to be executed. The implementation must support concurrent accesses.
        /// </summary>
        /// <returns>Next task from the list to be executed. Null if there is no task.</returns>
        public override TaskDecryption GetNextTask()
        {
            //todo: implement this method such that satisfies a thread safe shared buffer.
            TaskDecryption t = null;
            Console.WriteLine("in GetNextTask, get next task for worker");
            
            wSem.WaitOne();
            if (base.taskBuffer.Count > 0)
            {
                t = base.taskBuffer.Dequeue();
                if (t.id < 0) {
                    base.taskBuffer.Enqueue(t);
                }
            }
            pSem.Release();

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
            Provider[] providers = new Provider[numOfProviders];
            Worker[] workers = new Worker[numOfWorkers];

            // threadArr structute = Thread[provider/worker distinction][amount of provider/worker (is for provider, worker task is here)][provider task]
            Thread[][][] threadArr = new Thread[2][][];
            threadArr[0] = new Thread[numOfProviders][];
            threadArr[1] = new Thread[numOfWorkers][];

            // for-loop variable copied for function call-purposes
            int newK = 0;

            for (int i = 0; i < providers.Length; i++) {
                providers[i] = new Provider(tasks, base.challenges);
            }
            for (int i = 0; i < workers.Length; i++) {
                workers[i] = new Worker(tasks);
            }

            for (int i = 0; i < threadArr.Length; i++) {
                if (i == 0) {
                    for (int j = 0; j < threadArr[i].Length; j++) {
                        threadArr[i][j] = new Thread[base.challenges.Length + 1];
                        for (int k = 0; k < base.challenges.Length; k++) {
                            newK = k;
                            //threadArr[i][j][k] = new Thread(() => Console.WriteLine("newK={0} challenge={1} just general provider task", newK, base.challenges[newK]));
                            threadArr[i][j][k] = new Thread(() => tasks.AddTask(new TaskDecryption(newK, base.challenges[newK])));    
                        }
                        //threadArr[i][j][base.challenges.Length] = new Thread(() => Console.WriteLine("newK={0} challenge={1} should be terminating provider task", newK, base.challenges[newK]));
                        threadArr[i][j][base.challenges.Length] = new Thread(() => tasks.AddTask(new TaskDecryption(FixedParams.terminatingTaskId, "")));
                    }
                }
                else {
                    for (int j = 0; j < threadArr[i].Length; j++) {
                        threadArr[i][j] = new Thread[1];
                        threadArr[i][j][0] = new Thread(() => tasks.GetNextTask());
                        //threadArr[i][j][0] = new Thread(() => Console.WriteLine("executing: i={0}, j={1}", i, j));
                    }
                }
            }

            for (int i = 0; i < threadArr.Length; i++) {
                for (int j = 0; j < threadArr[i].Length; j++) {
                    for (int k = 0; k < threadArr[i][j].Length; k++) {
                        threadArr[i][j][k].Start();
                        Thread.Sleep(new Random().Next(WorkingParams.minSendIntervalTime, WorkingParams.maxSendIntervalTime));
                    }
                }
            }

            for (int i = 0; i < threadArr.Length; i++) {
                for (int j = 0; j < threadArr[i].Length; j++) {
                    for (int k = 0; k < threadArr[i][j].Length; k++) {
                        threadArr[i][j][k].Join();
                    }
                }
            }

            Console.WriteLine("thread test; everything is joined i hope");

            return tasks.GetLogs();
        }
    }
}
