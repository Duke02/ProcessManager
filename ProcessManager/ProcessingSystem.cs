using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ProcessManager
{
    /// <summary>
    /// A system that holds processors and organizes their execution.
    /// A real world example of this class would be a desktop computer or a distributed computer system.
    /// </summary>
    public class ProcessingSystem
    {
        /// <summary>
        /// The threads that hold each of the processors.
        /// </summary>
        private List<Thread> _processorThreads;

        /// <summary>
        /// Creates the System that simulates an operating system.
        /// </summary>
        /// <param name="numOfProcessors">The number of processors this system contains.</param>
        /// <param name="dispatchers">The dispatchers that the processors are to use. Length can be less than the number of processors.</param>
        public ProcessingSystem(int numOfProcessors, List<IDispatcher> dispatchers)
        {
            Processors = new List<Processor>(numOfProcessors);
            _processorThreads = new List<Thread>(numOfProcessors);

            for (var i = 0; i < numOfProcessors; i++)
            {
                Processors.Add(new Processor(dispatchers[i % dispatchers.Capacity], this));
            }

            GlobalQueue = new ConcurrentQueue<Process>();
        }

        /// <summary>
        /// The processors in the system.
        /// </summary>
        public List<Processor> Processors { get; }

        /// <summary>
        /// The global queue of ready processes in the system.
        /// </summary>
        public ConcurrentQueue<Process> GlobalQueue { get; }

        /// <summary>
        /// True if this system is currently executing processes throughout its processors.
        /// </summary>
        public bool IsOn { get; private set; }

        /// <summary>
        /// Turns on and starts the processors.
        /// </summary>
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

        /// <summary>
        /// Turns off the system and joins the processor threads.
        /// </summary>
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