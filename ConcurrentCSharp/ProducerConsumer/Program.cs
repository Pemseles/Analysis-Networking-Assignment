using Exercise;
using System;
using System.Threading;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            int wt = 1000;
            int n = 50;
            n = n + 2 - 2;

            Simulator simulator = new Simulator(10, wt);
            // todo 1: uncomment this and check the result. Analyze the related code.
            //simulator.sequentialOneProducerOneConsumer();

            //Console.ReadLine();

            // todo 2: uncomment this and check the result. Analyze the related code. Try with higher values for n.
            simulator.concurrentOneProducerOneConsumer(n);
        }
    }
}
