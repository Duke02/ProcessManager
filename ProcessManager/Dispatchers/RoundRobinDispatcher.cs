using System;
using System.Collections.Concurrent;

namespace ProcessManager.Dispatchers
{
    public class RoundRobinDispatcher : IDispatcher
    {
        private int _currentRunningPid;
        private int _runningStreak;

        public RoundRobinDispatcher(int timeQuantum)
        {
            TimeQuantum = timeQuantum;
        }

        public int TimeQuantum { get; }

        public bool IsPreemptive()
        {
            return true;
        }

        public bool ShouldPreempt(Processor processor, ProcessingSystem system)
        {
            if (processor.CurrentlyRunningProcess == null) return false;

            var currentProcessPid = processor.CurrentlyRunningProcess.ProcessId;
            return currentProcessPid == _currentRunningPid && _runningStreak >= TimeQuantum;
        }

        public Process Dispatch(Processor processor, ConcurrentQueue<Process> processQueue)
        {
            if (processQueue.IsEmpty) return null;

            Process result;

            do
            {
                processQueue.TryDequeue(out result);

                // If we just took the last process in the queue, let it be our output.
                if (processQueue.IsEmpty)
                    break;

                // If we got the same process, just throw it back in there.
                if (result != null && result.ProcessId == _currentRunningPid) processQueue.Enqueue(result);
            } while (result == null || result.ProcessId == _currentRunningPid);

            _currentRunningPid = result.ProcessId;
            _runningStreak = 0;

            return result;
        }

        public Func<Process, object> GetQueueOrder()
        {
            return process => process.AdmittedClockCycle;
        }

        public void UpdateOnClockCycle()
        {
            _runningStreak++;
        }
    }
}