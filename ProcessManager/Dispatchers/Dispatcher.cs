using System;
using System.Collections.Concurrent;

namespace ProcessManager.Dispatchers
{
    public interface IDispatcher
    {
        /// <summary>
        /// True if this Dispatcher takes processes out of their running state, False otherwise.
        /// </summary>
        /// <returns></returns>
        bool IsPreemptive();

        /// <summary>
        /// Checks if the processor's currently running process is to be taken out of the running state.
        /// </summary>
        /// <param name="processor">The processor that holds the running process.</param>
        /// <param name="system">The system the processor belongs to.</param>
        /// <returns>True if the currently running process should be preempted, False otherwise.</returns>
        bool ShouldPreempt(Processor processor, ProcessingSystem system);

        /// <summary>
        /// Selects the next process that is to be in the running state.
        /// </summary>
        /// <param name="processor">The processor that needs a new process to run.</param>
        /// <param name="processQueue">The queue to select the next process from.</param>
        /// <returns>The process that is to run next.</returns>
        Process Dispatch(Processor processor, ConcurrentQueue<Process> processQueue);

        /// <summary>
        /// Gets how the queue is supposed to be ordered in the system/processor.
        /// </summary>
        /// <returns>A lambda that specifies how the queue is supposed to be ordered.</returns>
        Func<Process, object> GetQueueOrder();

        /// <summary>
        /// Optional update on clock cycle for internal data.
        /// </summary>
        void UpdateOnClockCycle();
    }
}