using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ProcessManager
{
    public class Process
    {
        private int _executionStart;

        private static int _lastProcessId = 0;

        /// <summary>
        /// Creates the process with the given parameters.
        /// </summary>
        /// <param name="requiredCycles">The number of clock cycles this process requires to complete.</param>
        /// <param name="priority">The priority of this process.</param>
        public Process(int requiredCycles, int priority)
        {
            RequiredCycles = requiredCycles;
            Priority = priority;

            ProcessId = _lastProcessId;
            _lastProcessId++;

            ExecutionStart = -1;
        }

        /// <summary>
        /// <summary>
        /// The number of clock cycles HasCthat are required for this process to complete.
        /// </summary>
        public int RequiredCycles { get; }

        /// <summary>
        /// The priority of this process to complete.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// The id for this process.
        /// </summary>
        public int ProcessId { get; }

        /// <summary>
        /// The clock cycle that this process first started running.
        /// </summary>
        public int ExecutionStart
        {
            get => _executionStart;
            private set
            {
                if (_executionStart != default(int))
                {
                    Console.WriteLine(
                        $"WARNING: Process {ProcessId} has started execution more than once when it shouldn't have.");
                    return;
                }

                _executionStart = value;
            }
        }

        /// <summary>
        /// The clock cycle that this process completed.
        /// </summary>
        public int ExecutionEnd { get; private set; }

        /// <summary>
        /// The total number of clock cycles this process has been waiting since it began execution.
        /// </summary>
        public int TotalWait { get; private set; }

        /// <summary>
        /// The total number of clock cycles this process has been executing since it first started running.
        /// </summary>
        public int TotalExecution { get; private set; }

        /// <summary>
        /// True if the process has began execution, false otherwise.
        /// </summary>
        public bool BeganExecution => ExecutionStart != -1;

        /// <summary>
        /// True if the process has completed its work, false otherwise.
        /// </summary>
        public bool HasCompleted => TotalExecution >= RequiredCycles;

        /// <summary>
        /// The last clock cycle that this process executed.
        /// </summary>
        public int LastExecutionCycle { get; private set; }

        /// <summary>
        /// Runs the process.
        /// </summary>
        /// <param name="currentClockCycle">The current clock cycle of the system.</param>
        /// <returns>True if the process completed, false otherwise.</returns>
        public bool Run(int currentClockCycle)
        {
            if (BeganExecution)
            {
                TotalWait += currentClockCycle - LastExecutionCycle;
            }
            else
            {
                ExecutionStart = currentClockCycle;
            }

            LastExecutionCycle = currentClockCycle;
            TotalExecution += 1;

            if (HasCompleted)
            {
                ExecutionEnd = currentClockCycle;
            }

            return HasCompleted;
        }
    }
}