using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProcessManager
{
    /// <summary>
    /// An object that operates on processes and is a part of a system.
    /// A real world example of this class would be a core of a CPU, a CPU, or a computer in a distributed system.
    /// </summary>
    public class Processor
    {
        /// <summary>
        /// The system this processor belongs to.
        /// </summary>
        private readonly ProcessingSystem _system;

        private static int _lastId = 0;

        /// <summary>
        /// Field that holds the tabs to print debug information in a nicely formatted way.
        /// </summary>
        private readonly string _tabs;

        /// <summary>
        /// Creates the processor with the given parameters.
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
        }

        /// <summary>
        /// The algorithm that this processor uses to select the next process to run.
        /// </summary>
        public IDispatcher Dispatcher { get; }

        /// <summary>
        /// The local queue of ready processes that this processor can select from to run.
        /// </summary>
        public ConcurrentQueue<Process> LocalQueue { get; private set; }

        /// <summary>
        /// The currently running process on this processor.
        /// </summary>
        public Process CurrentlyRunningProcess { get; private set; }

        /// <summary>
        /// The identification number of this processor.
        /// </summary>
        public int ProcessorId { get; }

        /// <summary>
        /// True if there is no currently running process, False otherwise.
        /// </summary>
        public bool IsIdling => CurrentlyRunningProcess == null;

        /// <summary>
        /// The current clock cycle of this processor.
        /// </summary>
        public int CurrentClockCycle { get; private set; }

        public bool IsDone => GetAppropriateQueue().IsEmpty && IsIdling;

        /// <summary>
        /// Gets the queue that new processes are to come from.
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
        /// Adds to the queue and orders it.
        /// </summary>
        /// <param name="process">The process to be added to the queue.</param>
        public void AddToLocalQueue(Process process)
        {
            LocalQueue.Enqueue(process);
            SortTheQueue();
        }

        /// <summary>
        /// Adds all of the given processes to the local queue.
        /// </summary>
        /// <param name="processes">The processes to add.</param>
        public void AddToLocalQueue(IEnumerable<Process> processes)
        {
            foreach (var process in processes)
            {
                LocalQueue.Enqueue(process);
            }

            SortTheQueue();
        }

        /// <summary>
        /// Sorts the local queue.
        /// </summary>
        public void SortTheQueue()
        {
            LocalQueue = new ConcurrentQueue<Process>(LocalQueue.OrderBy(Dispatcher.GetQueueOrder()));
        }

        /// <summary>
        /// Prints a message to the console using the given message.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        private void PrintInformation(string message)
        {
            Console.WriteLine($"{_tabs}Processor {ProcessorId} @ Cycle {CurrentClockCycle}: {message}");
        }

        /// <summary>
        /// Gets the process to run and allows it to run.
        /// </summary>
        public void Process()
        {
            PrintInformation("Beginning cycle.");

            if (Dispatcher.IsPreemptive() && Dispatcher.ShouldPreempt(this, _system))
            {
                // TODO: Make sure this doesn't make the enqueued process null again.
                var preemptedProcess = CurrentlyRunningProcess;
                // TODO: Make sure this actually enqueues it.
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


            if (processHasCompleted)
            {
                PrintInformation("Current process has completed.");
                CurrentlyRunningProcess = null;
            }

            PrintInformation("Completed cycle.");

            CurrentClockCycle += 1;
        }
    }
}