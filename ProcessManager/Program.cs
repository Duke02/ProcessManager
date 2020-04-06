﻿using System.Collections.Generic;
using ProcessManager.Dispatchers;

namespace ProcessManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var numOfProcessors = 5;
            var dispatchers = new List<IDispatcher>();

            for (var i = 0; i < numOfProcessors; i++) dispatchers.Add(new RoundRobinDispatcher(4));

            var system = new ProcessingSystem(numOfProcessors, dispatchers);
            system.Simulate();
        }
    }
}