using System;
using System.Collections.Generic;

namespace ProcessManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = new ProcessingSystem(1, new List<IDispatcher>() {new FirstInFirstOutDispatcher()});
            system.Simulate();
            
        }
    }
}