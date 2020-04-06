using System;
using System.Collections.Generic;

namespace ProcessManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var numOfProcessors = 5;
            var dispatchers = new List<IDispatcher>();

            for (var i = 0; i < numOfProcessors; i++)
            {
                dispatchers.Add(new FirstInFirstOutDispatcher());
            }

            var system = new ProcessingSystem(numOfProcessors, dispatchers);
            system.Simulate();
        }
    }
}