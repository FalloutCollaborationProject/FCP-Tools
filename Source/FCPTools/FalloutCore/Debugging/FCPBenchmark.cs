using System.Diagnostics;
using System.Threading;

namespace FCP.Core
{
    /// <summary>
    /// Utility class for running somewhat accurate-ish... maybe.. benchmarks
    /// </summary>
    public static class FCPBenchmark
    {
        /// <summary>
        /// Profile a given action.
        /// </summary>
        /// <param name="iterations">Number of loops</param>
        /// <param name="func">Function to be ran</param>
        /// <param name="average">The averate time taken over all functions in ms (result / iterations)</param>
        /// <returns>Total Milliseconds over all loops</returns>
        public static double Profile(int iterations, Action func, out double average) {
            //Run at highest priority to minimize fluctuations caused by other processes/threads
            ProcessPriorityClass originalPrioClass = Process.GetCurrentProcess().PriorityClass;
            ThreadPriority originalPrio = Thread.CurrentThread.Priority;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            // warm up 
            func();
            Stopwatch watch = new (); 

            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (int i = 0; i < iterations; i++) {
                func();
            }
            watch.Stop();
        
            Process.GetCurrentProcess().PriorityClass = originalPrioClass;
            Thread.CurrentThread.Priority = originalPrio;
        
            average = watch.ElapsedMilliseconds / (float)iterations;
            return watch.ElapsedMilliseconds;
        }
    }
}