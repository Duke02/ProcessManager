using System;
using System.Collections.Concurrent;
using System.Linq;

namespace ProcessManager.Dispatchers
{
    public class ShortestRemainingCyclesDispatcher : IDispatcher
    {
        public bool IsPreemptive()
        {
            return true;
        }

        public bool ShouldPreempt(Processor processor, ProcessingSystem system)
        {
            return processor.CurrentlyRunningProcess != null && processor.LocalQueue.Any(process =>
                process.CyclesRemaining < processor.CurrentlyRunningProcess.CyclesRemaining);
        }

        public Process Dispatch(Processor processor, ConcurrentQueue<Process> processQueue)
        {
            if (processQueue.IsEmpty)
                return null;

            processQueue.TryDequeue(out var result);
            return result;
        }

        public Func<Process, object> GetQueueOrder()
        {
            return process => process.CyclesRemaining;
        }

        public void UpdateOnClockCycle()
        {
        }
    }
}