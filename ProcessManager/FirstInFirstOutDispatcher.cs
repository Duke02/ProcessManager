using System;
using System.Collections.Concurrent;
using System.Linq;

namespace ProcessManager
{
    public class FirstInFirstOutDispatcher : IDispatcher
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
            processor.SortTheQueue();
            processor.LocalQueue.TryDequeue(out var result);
            return result;
        }

        public Func<Process, object> GetQueueOrder()
        {
            return process => process.AdmittedClockCycle;
        }
    }
}