using System;

namespace BrewerySimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            BreweryWorker worker = new BreweryWorker();
            worker.Start();

            Console.ReadKey();
        }
    }
}
