using System.Collections.Generic;
using ProcessManager.Dispatchers;

namespace ProcessManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var dispatchers = new List<IDispatcher>();

            dispatchers.Add(new FirstInFirstOutDispatcher());
            dispatchers.Add(new ShortestProcessNextDispatcher());
            dispatchers.Add(new ShortestRemainingCyclesDispatcher());
            dispatchers.Add(new RoundRobinDispatcher(2));
            dispatchers.Add(new RoundRobinDispatcher(4));
            dispatchers.Add(new RoundRobinDispatcher(8));

            var numOfProcessors = dispatchers.Count;

            var system = new ProcessingSystem(numOfProcessors, dispatchers);
            system.Simulate();
        }
    }
}