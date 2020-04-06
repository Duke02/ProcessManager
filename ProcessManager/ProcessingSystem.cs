using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ProcessManager
{
    public class ProcessingSystem
    {
        private List<Thread> _processorThreads;

        public ProcessingSystem(int numOfProcessors, List<IDispatcher> dispatchers)
        {
            Processors = new List<Processor>(numOfProcessors);
            _processorThreads = new List<Thread>(numOfProcessors);

            for (var i = 0; i < numOfProcessors; i++)
            {
                Processors.Add(new Processor(dispatchers[i], this));
            }

            GlobalQueue = new ConcurrentQueue<Process>();
        }

        public List<Processor> Processors { get; }

        public ConcurrentQueue<Process> GlobalQueue { get; }

        public bool IsOn { get; private set; }

        public void TurnOn()
        {
            IsOn = true;
            foreach (var processor in Processors)
            {
                _processorThreads.Add(new Thread(() =>
                {
                    while (IsOn)
                    {
                        processor.Process();
                    }
                }));
            }
        }

        public void TurnOff()
        {
            IsOn = false;

            foreach (var processorThread in _processorThreads)
            {
                processorThread.Join();
            }
        }
    }
}