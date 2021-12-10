using System;
using System.Diagnostics;
using System.Threading;
using Exercise;

namespace Concurrent
{
    public class ConPrimeNumbers : PrimeNumbers
    {
        /// <summary>
        /// This method 
        /// </summary>
        /// <param name="m"> is the minimum number</param>
        /// <param name="M"> is the maximum number</param>
        /// <param name="nt"> is the number of threads. For simplicity assume two.</param>
        public void runConcurrent(int m, int M)
        {
            // Todo 1: Create two threads, define their segments and start them. Join them all to have all the work done.
            int nt = 100;
            Stopwatch sw = new Stopwatch();
            
            int numTs = nt;
            int s = (M - m) / nt;
            
            Thread[] threadArr = new Thread[numTs];

            for (int i = 0; i < numTs; i++) {
                int l = m + s * i;
                int u = 0;
                if (i == numTs - 1)
                    u = M;
                else
                    u = m + s * (i + 1);

                threadArr[i] = new Thread(() => PrimeNumbers.printPrimes(l, u));
            }
            sw.Start();
            for (int i = 0; i < numTs; i++) {
                threadArr[i].Start();
            }

            for (int i = 0; i < numTs; i++) {
                threadArr[i].Join();
            }
            sw.Stop();
            Console.WriteLine("concurrent time elapsed w {0} threads: {1} seconds", nt, sw.Elapsed);
        }

    }
}
