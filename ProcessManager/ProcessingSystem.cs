using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ProcessManager.Dispatchers;

namespace ProcessManager
{
    /// <summary>
    ///     A system that holds processors and organizes their execution.
    ///     A real world example of this class would be a desktop computer or a distributed computer system.
    /// </summary>
    public class ProcessingSystem
    {
        /// <summary>
        ///     The threads that hold each of the processors.
        /// </summary>
        private readonly List<Thread> _processorThreads;

        /// <summary>
        ///     Creates the System that simulates an operating system.
        /// </summary>
        /// <param name="numOfProcessors">The number of processors this system contains.</param>
        /// <param name="dispatchers">
        ///     The dispatchers that the processors are to use. Length can be less than the number of
        ///     processors.
        /// </param>
        public ProcessingSystem(int numOfProcessors, List<IDispatcher> dispatchers)
        {
            ProcessProducer = new Producer(new Random().Next(20, 40) * numOfProcessors);

            Processors = new List<Processor>(numOfProcessors);
            _processorThreads = new List<Thread>(numOfProcessors);

            for (var i = 0; i < numOfProcessors; i++) Processors.Add(new Processor(dispatchers[i], this));

            GlobalQueue = new ConcurrentQueue<Process>();
        }

        /// <summary>
        ///     The object that produces processes throughout the system.
        /// </summary>
        public Producer ProcessProducer { get; }

        /// <summary>
        ///     The processors in the system.
        /// </summary>
        public List<Processor> Processors { get; }

        /// <summary>
        ///     The global queue of ready processes in the system.
        /// </summary>
        public ConcurrentQueue<Process> GlobalQueue { get; }

        /// <summary>
        ///     True if this system is currently executing processes throughout its processors.
        /// </summary>
        public bool IsOn { get; private set; }

        /// <summary>
        ///     Turns on and starts the processors.
        /// </summary>
        public void TurnOn()
        {
            Console.WriteLine("Turning system on...");
            IsOn = true;
            foreach (var processor in Processors)
                _processorThreads.Add(new Thread(() =>
                {
                    while (IsOn) processor.Process();
                }));

            foreach (var processorThread in _processorThreads) processorThread.Start();
        }

        /// <summary>
        ///     Turns off the system and joins the processor threads.
        /// </summary>
        public void TurnOff()
        {
            Console.WriteLine("Turning System off...");
            IsOn = false;

            foreach (var processorThread in _processorThreads) processorThread.Join();

            Console.WriteLine("System is off.");
        }

        /// <summary>
        ///     Adds processes to each producer by at most the given amount.
        /// </summary>
        /// <param name="numToAddPerProcessor">The maximum number of processes to allot to each processor.</param>
        private void AddProcessesToProcessors(int numToAddPerProcessor)
        {
            foreach (var processor in Processors)
                if (ProcessProducer.CanProduce(numToAddPerProcessor))
                {
                    processor.AddToLocalQueue(ProcessProducer.ProduceProcesses(numToAddPerProcessor, 0));
                }
                else if (ProcessProducer.IsDoneProducing)
                {
                    break;
                }
                else
                {
                    var producerCanProduce = ProcessProducer.ProcessesCanProduce;
                    processor.AddToLocalQueue(ProcessProducer.ProduceProcesses(producerCanProduce, 0));
                }
        }

        private void AddProcessesToIdleProcessors(int numToAdd)
        {
            var processors = Processors;
            var idlingProcessors = processors.Where(processor => processor.IsIdling);
            var idlingProcessorsAsList = idlingProcessors.ToList();
            foreach (var processor in idlingProcessorsAsList)
            {
                if (ProcessProducer.CanProduce(numToAdd))
                {
                    processor.AddToLocalQueue(ProcessProducer.ProduceProcesses(numToAdd, processor.CurrentClockCycle));
                }
                else if (!ProcessProducer.IsDoneProducing)
                {
                    var leftoverProcesses = ProcessProducer.ProcessesCanProduce;
                    processor.AddToLocalQueue(
                        ProcessProducer.ProduceProcesses(leftoverProcesses, processor.CurrentClockCycle));
                }
                else if (ProcessProducer.IsDoneProducing)
                {
                    break;
                }
            }

            Processors.RemoveAll(processor => processor.IsIdling);
            Processors.AddRange(idlingProcessorsAsList);
        }

        /// <summary>
        ///     Simulates a real world computing environment.
        /// </summary>
        public void Simulate()
        {
            Console.WriteLine("Beginning simulation.");

            AddProcessesToProcessors(5);

            TurnOn();

            // Wait until producer cannot create any more processes for processors.
            while (!ProcessProducer.IsDoneProducing)
            {
                Thread.Sleep(Constants.ClockPeriod);
                if (Processors.Any(processor => processor.IsIdling)) AddProcessesToIdleProcessors(5);
            }

            Console.WriteLine("Waiting until processors are finished with current tasks.");

            // Wait until all processors are done.
            while (Processors.Any(processor => !processor.IsDone)) Thread.Sleep(Constants.ClockPeriod);

            TurnOff();
            Console.WriteLine("Simulation has finished.");

            PrintStatistics();
        }

        private void PrintStatistics()
        {
            Console.WriteLine("Calculating Statistics...");

            var processorStats = Processors.Select(processor => processor.CalculateStatistics()).ToList();

            foreach (var stat in processorStats) Console.WriteLine($"\n{stat.ToString()}\n");

            var sumOfTotalClockCycles = processorStats.Sum(processor => processor.TotalClockCycles);

            var averageClockCycles = processorStats.Average(processor => processor.TotalClockCycles);
            var totalProcessesSeen = processorStats.Sum(processor => processor.TotalProcessesSeen);
            var averageTurnaroundTime =
                processorStats.Sum(processor => processor.AverageTurnaroundTime * processor.TotalClockCycles) /
                sumOfTotalClockCycles;
            var averageNTat =
                processorStats.Sum(processor =>
                    processor.AverageNormalizedTurnaroundTime * processor.TotalClockCycles) / sumOfTotalClockCycles;
            var averageWaitCycles = processorStats.Sum(stat => stat.AverageWaitCycles * stat.TotalClockCycles) /
                                    sumOfTotalClockCycles;
            var averageServiceTime = processorStats.Sum(stat => stat.AverageServiceTime * stat.TotalClockCycles) /
                                     sumOfTotalClockCycles;

            var sb = new StringBuilder();
            sb.Append('=', 25);
            Console.WriteLine(sb.ToString());

            Console.WriteLine($"System with {Processors.Count} processors encountered the following statistics.");
            Console.WriteLine($"Total Processes Seen: {totalProcessesSeen}");
            Console.WriteLine($"Average Clock Cycles: {averageClockCycles}");
            Console.WriteLine($"Average Service Time: {averageServiceTime}");
            Console.WriteLine($"Average Turnaround Time: {averageTurnaroundTime}");
            Console.WriteLine($"Average Normalized Turnaround Time: {averageNTat}");
            Console.WriteLine($"Average Wait Cycles: {averageWaitCycles}");
        }
    }
}