using System;
using System.Collections.Concurrent;

namespace ProcessManager.Dispatchers
{
    public class ShortestProcessNextDispatcher : IDispatcher
    {
        public bool IsPreemptive()
        {
            return false;
        }

        public bool ShouldPreempt(Processor processor, ProcessingSystem system)
        {
            return false;
        }

        public Process Dispatch(Processor processor, ConcurrentQueue<Process> processQueue)
        {
            processQueue.TryDequeue(out var result);
            return result;
        }

        public Func<Process, object> GetQueueOrder()
        {
            return process => process.RequiredCycles;
        }

        public void UpdateOnClockCycle()
        {
        }
    }
}