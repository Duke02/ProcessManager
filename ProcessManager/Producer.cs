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
        /// <summary>
        /// The random number generator responsible for randomizing each element of the new processes.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Creates a producer with the max number of processes to produce.
        /// </summary>
        /// <param name="maxToProduce">The maximum number of processes to produce from this producer.</param>
        public Producer(int maxToProduce)
        {
            _random = new Random();
            MaxProcessesToProduce = maxToProduce;
            ProcessesProduced = 0;
        }

        /// <summary>
        /// The maximum number of processes this producer will create.
        /// </summary>
        public int MaxProcessesToProduce { get; }

        /// <summary>
        /// The number of processes this producer has created so far.
        /// </summary>
        public int ProcessesProduced { get; private set; }

        /// <summary>
        /// True if the producer has made at least the same number of processes as its maximum. 
        /// </summary>
        public bool IsDoneProducing => ProcessesProduced >= MaxProcessesToProduce;

        /// <summary>
        /// The number of processes that are still to be produced.
        /// </summary>
        public int ProcessesCanProduce => MaxProcessesToProduce - ProcessesProduced;

        /// <summary>
        /// Produces a new process.
        /// </summary>
        /// <param name="currentClockCycle">The current clock cycle, to be used for the process's admittance into the system.</param>
        /// <returns>The newly created process.</returns>
        public Process ProduceProcess(int currentClockCycle)
        {
            ProcessesProduced++;

            var requiredCycles = _random.Next(2, 20);
            var priority = _random.Next(10, 99);

            return new Process(requiredCycles, priority, currentClockCycle);
        }


        /// <summary>
        /// Checks if the producer can produce the requested number of processes.
        /// </summary>
        /// <param name="numToAdd">The requested number of processes to add.</param>
        /// <returns>True if the producer has enough left over processes to create, False otherwise.</returns>
        public bool CanProduce(int numToAdd)
        {
            return numToAdd <= ProcessesCanProduce;
        }

        /// <summary>
        /// Produces at most the given number of processes. 
        /// </summary>
        /// <param name="numToAdd">The maximum number of processes that will be created.</param>
        /// <param name="currentClockCycle">The current clock cycle, to be used for the admittance of the process.</param>
        /// <returns>The collection of new processes.</returns>
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