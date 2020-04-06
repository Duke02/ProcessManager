using System;
using System.Collections.Generic;

namespace ProcessManager
{
    /// <summary>
    /// Produces Processes.
    /// A real world example of this class would be a user operating the system.
    /// </summary>
    public class Producer
    {
        private Random _random;

        public Producer(int maxToProduce)
        {
            _random = new Random();
            MaxProcessesToProduce = maxToProduce;
            ProcessesProduced = 0;
        }

        public int MaxProcessesToProduce { get; }

        public int ProcessesProduced { get; private set; }

        public bool IsDoneProducing => ProcessesProduced >= MaxProcessesToProduce;

        public int ProcessesCanProduce => MaxProcessesToProduce - ProcessesProduced;

        public Process ProduceProcess(int currentClockCycle)
        {
            ProcessesProduced++;

            var requiredCycles = _random.Next(2, 20);
            var priority = _random.Next(10, 99);

            return new Process(requiredCycles, priority, currentClockCycle);
        }


        public bool CanProduce(int numToAdd)
        {
            return numToAdd <= ProcessesCanProduce;
        }

        public IEnumerable<Process> ProduceProcesses(int numToAdd, int currentClockCycle)
        {
            var output = new List<Process>();

            for (var i = 0; i < numToAdd; i++)
            {
                if (!CanProduce(1))
                {
                    break;
                }

                output.Add(ProduceProcess(currentClockCycle));
            }

            return output;
        }
    }
}