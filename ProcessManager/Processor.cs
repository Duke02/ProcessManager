using System;
using System.Collections.Concurrent;
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
        public ConcurrentQueue<Process> LocalQueue { get; }

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

        private void PrintInformation(string message)
        {
            Console.WriteLine($"{DateTime.Now}|Processor {ProcessorId}: {message}");
        }

        public void Process()
        {
            if (Dispatcher.IsPreemptive() && Dispatcher.ShouldPreempt(this, _system))
            {
                // TODO: Make sure this doesn't make the enqueued process null again.
                var preemptedProcess = CurrentlyRunningProcess;
                // TODO: Make sure this actually enqueues it.
                GetAppropriateQueue().Enqueue(preemptedProcess);
                CurrentlyRunningProcess = null;
            }

            if (IsIdling)
            {
                CurrentlyRunningProcess = Dispatcher.Dispatch(this, GetAppropriateQueue());
            }

            var processHasCompleted = CurrentlyRunningProcess != null && CurrentlyRunningProcess.Run(CurrentClockCycle);

            if (processHasCompleted)
            {
                CurrentlyRunningProcess = null;
            }

            Thread.Sleep(Constants.ClockPeriod);

            CurrentClockCycle += 1;
        }
    }
}