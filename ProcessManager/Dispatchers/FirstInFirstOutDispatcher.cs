using System;
using System.Collections.Concurrent;

namespace ProcessManager.Dispatchers
{
    public class FirstInFirstOutDispatcher : IDispatcher
    {
        /// <summary>
        ///     As FIFO is a non preemptive policy, this always returns false.
        /// </summary>
        /// <returns>False.</returns>
        public bool IsPreemptive()
        {
            return false;
        }

        /// <summary>
        ///     As the FIFO policy is non-preemptive, this always returns false.
        /// </summary>
        /// <param name="processor">The processor that uses this policy.</param>
        /// <param name="system">The system of the processor.</param>
        /// <returns>False.</returns>
        public bool ShouldPreempt(Processor processor, ProcessingSystem system)
        {
            return false;
        }

        public Process Dispatch(Processor processor, ConcurrentQueue<Process> processQueue)
        {
            processor.SortTheQueue();
            processor.LocalQueue.TryDequeue(out var result);
            return result;
        }

        /// <summary>
        ///     Returns the order that the queue is to go in.
        /// </summary>
        /// <returns>An ordering based on when the process was admitted into the system.</returns>
        public Func<Process, object> GetQueueOrder()
        {
            return process => process.AdmittedClockCycle;
        }

        public void UpdateOnClockCycle()
        {
        }

        public string Name => "First In First Out";
    }
}