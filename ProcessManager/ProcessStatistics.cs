namespace ProcessManager
{
    public struct ProcessStatistics
    {
        /// <summary>
        ///     The number of clock cycles it takes for a process to get admitted then complete within a system.
        /// </summary>
        public int TurnaroundTime;

        /// <summary>
        ///     The normalized turnaround time. Basically, turnaround time divided by the number of cycles required for a process
        ///     to complete.
        /// </summary>
        public double NormalizedTurnaroundTime;

        /// <summary>
        ///     The total number of clock cycles that the process spent waiting to run.
        /// </summary>
        public int TotalWaitTime;

        /// <summary>
        ///     The total number of clock cycles that the process needed to complete.
        /// </summary>
        public int ServiceTime;
    }
}