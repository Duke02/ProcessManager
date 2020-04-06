using System;

namespace ProcessManager
{
    public class Process
    {
        private static int _lastProcessId;
        private int _executionStart;

        /// <summary>
        ///     Creates the process with the given parameters.
        /// </summary>
        /// <param name="requiredCycles">The number of clock cycles this process requires to complete.</param>
        /// <param name="priority">The priority of this process.</param>
        /// <param name="currentClockCycle">The current clock cycle of the processor.</param>
        public Process(int requiredCycles, int priority, int currentClockCycle)
        {
            RequiredCycles = requiredCycles;
            Priority = priority;
            AdmittedClockCycle = currentClockCycle;

            ProcessId = _lastProcessId;
            _lastProcessId++;

            _executionStart = -1;
        }

        /// <summary>
        ///     The number of clock cycles that are required for this process to complete.
        /// </summary>
        public int RequiredCycles { get; }

        /// <summary>
        ///     The priority of this process to complete.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        ///     The id for this process.
        /// </summary>
        public int ProcessId { get; }

        /// <summary>
        ///     The clock cycle that this process first started running.
        /// </summary>
        public int ExecutionStart
        {
            get => _executionStart;
            private set
            {
                if (_executionStart != -1)
                {
                    Console.WriteLine(
                        $"WARNING: Process {ProcessId} has started execution more than once when it shouldn't have.");
                    return;
                }

                _executionStart = value;
            }
        }

        /// <summary>
        ///     The clock cycle that this process completed.
        /// </summary>
        public int ExecutionEnd { get; private set; }

        /// <summary>
        ///     The total number of clock cycles this process has been waiting since it began execution.
        /// </summary>
        public int TotalWait { get; private set; }

        /// <summary>
        ///     The total number of clock cycles this process has been executing since it first started running.
        /// </summary>
        public int TotalExecution { get; private set; }

        /// <summary>
        ///     The clock cycle this process was admitted into the system.
        /// </summary>
        public int AdmittedClockCycle { get; }

        /// <summary>
        ///     True if the process has began execution, false otherwise.
        /// </summary>
        public bool BeganExecution => ExecutionStart != -1;

        /// <summary>
        ///     True if the process has completed its work, false otherwise.
        /// </summary>
        public bool HasCompleted => TotalExecution >= RequiredCycles;

        /// <summary>
        ///     The last clock cycle that this process executed.
        /// </summary>
        public int LastExecutionCycle { get; private set; }

        /// <summary>
        ///     Prints debug information for this process.
        /// </summary>
        /// <param name="message">The message to print.</param>
        private void PrintInformation(string message)
        {
            Console.WriteLine($"Process {ProcessId}: {message}");
        }

        /// <summary>
        ///     Runs the process.
        /// </summary>
        /// <param name="currentClockCycle">The current clock cycle of the system.</param>
        /// <returns>True if the process completed, false otherwise.</returns>
        public bool Run(int currentClockCycle)
        {
            PrintInformation($"Running at clock cycle {currentClockCycle}");

            if (BeganExecution)
                TotalWait += currentClockCycle - LastExecutionCycle - 1;
            else
                ExecutionStart = currentClockCycle;

            LastExecutionCycle = currentClockCycle;
            TotalExecution += 1;

            if (HasCompleted) ExecutionEnd = currentClockCycle;

            return HasCompleted;
        }

        /// <summary>
        ///     Calculates the statistics for this process.
        /// </summary>
        /// <param name="turnAroundTime">The number of cycles that this process took to get in the system and out.</param>
        /// <param name="normalizedTurnAroundTime">turnAroundTime / Service time</param>
        public void CalculateStatistics(out int turnAroundTime, out double normalizedTurnAroundTime)
        {
            var test = TotalExecution + TotalWait;
            turnAroundTime = ExecutionEnd - AdmittedClockCycle + 1;
            normalizedTurnAroundTime = turnAroundTime / (double) RequiredCycles;
        }

        public void CalculateStatistics(out ProcessStatistics statistics)
        {
            CalculateStatistics(out var tat, out var nTat);
            statistics = new ProcessStatistics
            {
                TurnaroundTime = tat,
                NormalizedTurnaroundTime = nTat,
                TotalWaitTime = TotalWait,
                ServiceTime = RequiredCycles
            };
        }
    }
}