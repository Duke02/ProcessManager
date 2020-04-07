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
    ///     An object that operates on processes and is a part of a system.
    ///     A real world example of this class would be a core of a CPU, a CPU, or a computer in a distributed system.
    /// </summary>
    public class Processor
    {
        private static int _lastId;

        private readonly List<ProcessStatistics> _statisticHistory;

        /// <summary>
        ///     The system this processor belongs to.
        /// </summary>
        private readonly ProcessingSystem _system;

        /// <summary>
        ///     Field that holds the tabs to print debug information in a nicely formatted way.
        /// </summary>
        private readonly string _tabs;

        /// <summary>
        ///     Creates the processor with the given parameters.
        /// </summary>
        /// <param name="dispatcher">The algorithm this processor will use to choose the next process to run.</param>
        /// <param name="system">The system that this processor is a part of.</param>
        public Processor(IDispatcher dispatcher, ProcessingSystem system)
        {
            Dispatcher = dispatcher;
            _system = system;

            ProcessorId = _lastId;
            _lastId++;

            var tabsBuilder = new StringBuilder();
            tabsBuilder.Append('\t', ProcessorId + 1);
            _tabs = tabsBuilder.ToString();

            LocalQueue = new ConcurrentQueue<Process>();
            CurrentClockCycle = 0;

            ProcessesSeen = 0;
            _statisticHistory = new List<ProcessStatistics>();
        }

        /// <summary>
        ///     The algorithm that this processor uses to select the next process to run.
        /// </summary>
        public IDispatcher Dispatcher { get; }

        /// <summary>
        ///     The local queue of ready processes that this processor can select from to run.
        /// </summary>
        public ConcurrentQueue<Process> LocalQueue { get; private set; }

        /// <summary>
        ///     The currently running process on this processor.
        /// </summary>
        public Process CurrentlyRunningProcess { get; private set; }

        /// <summary>
        ///     The identification number of this processor.
        /// </summary>
        public int ProcessorId { get; }

        /// <summary>
        ///     The total number of processes that this processor has seen.
        /// </summary>
        public int ProcessesSeen { get; private set; }

        /// <summary>
        ///     True if there is no currently running process, False otherwise.
        /// </summary>
        public bool IsIdling => CurrentlyRunningProcess == null;

        /// <summary>
        ///     The current clock cycle of this processor.
        /// </summary>
        public int CurrentClockCycle { get; private set; }

        /// <summary>
        ///     True if the queue is empty and the processor is idling, False otherwise.
        /// </summary>
        public bool IsDone => GetAppropriateQueue().IsEmpty && IsIdling;

        /// <summary>
        ///     The average turnaround time that the processor has seen.
        /// </summary>
        public double AverageTurnaroundTime => _statisticHistory.Average(stat => stat.TurnaroundTime);

        /// <summary>
        ///     The average normalized turnaround time that the processor has seen.
        /// </summary>
        public double AverageNormalizedTurnaroundTime =>
            _statisticHistory.Average(stat => stat.NormalizedTurnaroundTime);

        /// <summary>
        ///     The average service time each process on this processor required to take to complete.
        /// </summary>
        public double AverageServiceTime => _statisticHistory.Average(stat => stat.ServiceTime);

        /// <summary>
        ///     The average cycles that each process on the processor spent waiting.
        /// </summary>
        public double AverageWaitCycles => _statisticHistory.Average(stat => stat.TotalWaitTime);

        /// <summary>
        ///     Gets the queue that new processes are to come from.
        /// </summary>
        /// <returns></returns>
        private ConcurrentQueue<Process> GetAppropriateQueue()
        {
            // For now, this will return our local queue.
            // If we decide to make this use the system queue,
            // this is what we will change.
            return LocalQueue;
        }

        /// <summary>
        ///     Adds to the queue and orders it.
        /// </summary>
        /// <param name="process">The process to be added to the queue.</param>
        public void AddToLocalQueue(Process process)
        {
            LocalQueue.Enqueue(process);
            SortTheQueue();
        }

        /// <summary>
        ///     Adds all of the given processes to the local queue.
        /// </summary>
        /// <param name="processes">The processes to add.</param>
        public void AddToLocalQueue(IEnumerable<Process> processes)
        {
            foreach (var process in processes) LocalQueue.Enqueue(process);

            SortTheQueue();
        }

        /// <summary>
        ///     Sorts the local queue.
        /// </summary>
        public void SortTheQueue()
        {
            LocalQueue = new ConcurrentQueue<Process>(LocalQueue.OrderBy(Dispatcher.GetQueueOrder()));
        }

        /// <summary>
        ///     Prints a message to the console using the given message.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        private void PrintInformation(string message)
        {
            Console.WriteLine($"{_tabs}Processor {ProcessorId} @ Cycle {CurrentClockCycle}: {message}");
        }

        /// <summary>
        ///     Gets the process to run and allows it to run.
        /// </summary>
        public void Process()
        {
            PrintInformation("Beginning cycle.");

            if (Dispatcher.IsPreemptive() && Dispatcher.ShouldPreempt(this, _system))
            {
                var preemptedProcess = CurrentlyRunningProcess;
                GetAppropriateQueue().Enqueue(preemptedProcess);
                CurrentlyRunningProcess = null;
                PrintInformation("Preempted Process.");
            }

            if (IsIdling)
            {
                PrintInformation("Idling...");
                CurrentlyRunningProcess = Dispatcher.Dispatch(this, GetAppropriateQueue());
            }

            Thread.Sleep(Constants.ClockPeriod);

            var processHasCompleted = CurrentlyRunningProcess != null && CurrentlyRunningProcess.Run(CurrentClockCycle);

            Dispatcher.UpdateOnClockCycle();

            if (processHasCompleted)
            {
                PrintInformation("Current process has completed.");

                ProcessesSeen++;
                CurrentlyRunningProcess.CalculateStatistics(out var processStats);
                _statisticHistory.Add(processStats);

                CurrentlyRunningProcess = null;
            }

            PrintInformation("Completed cycle.");

            CurrentClockCycle += 1;
        }

        public ProcessorStatistics CalculateStatistics()
        {
            return new ProcessorStatistics
            {
                AverageServiceTime = AverageServiceTime,
                ProcessorId = ProcessorId,
                TotalProcessesSeen = ProcessesSeen,
                AverageTurnaroundTime = AverageTurnaroundTime,
                AverageWaitCycles = AverageWaitCycles,
                AverageNormalizedTurnaroundTime = AverageNormalizedTurnaroundTime,
                TotalClockCycles = CurrentClockCycle
            };
        }
    }

    public struct ProcessorStatistics
    {
        /// <summary>
        ///     The id for the processor that these statistics correspond with.
        /// </summary>
        public int ProcessorId;

        /// <summary>
        ///     The total number of processes the processor has seen.
        /// </summary>
        public int TotalProcessesSeen;

        /// <summary>
        ///     The average turnaround time for each process to enter and leave the system on the processor.
        /// </summary>
        public double AverageTurnaroundTime;

        /// <summary>
        ///     The average normalized turnaround time for each process on the processor.
        /// </summary>
        public double AverageNormalizedTurnaroundTime;

        /// <summary>
        ///     The average number of cycles the processes on the processor took waiting.
        /// </summary>
        public double AverageWaitCycles;

        /// <summary>
        ///     The average number of cycles the processes on the processor required to complete.
        /// </summary>
        public double AverageServiceTime;

        public int TotalClockCycles;

        public override string ToString()
        {
            return $"Processor {ProcessorId} had the following statistics:" +
                   $"\n\t{TotalClockCycles} total clock cycles" +
                   $"\n\t{TotalProcessesSeen} total processes seen" +
                   $"\n\t{AverageTurnaroundTime} average turnaround time" +
                   $"\n\t{AverageNormalizedTurnaroundTime} average normalized time" +
                   $"\n\t{AverageServiceTime} average service time" +
                   $"\n\t{AverageWaitCycles} average wait cycles";
        }
    }
}