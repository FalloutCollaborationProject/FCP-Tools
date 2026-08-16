using System.Diagnostics;
using System.Threading;

namespace FCP.Core;

public static class FCPBenchmark
{
    public static double Profile(int iterations, Action func, out double average)
    {
        ProcessPriorityClass originalPrioClass = Process.GetCurrentProcess().PriorityClass;
        ThreadPriority originalPrio = Thread.CurrentThread.Priority;
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        func();
        Stopwatch watch = new ();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        watch.Start();
        for (int i = 0; i < iterations; i++)
        {
            func();
        }
        watch.Stop();

        Process.GetCurrentProcess().PriorityClass = originalPrioClass;
        Thread.CurrentThread.Priority = originalPrio;

        average = watch.ElapsedMilliseconds / (float)iterations;
        return watch.ElapsedMilliseconds;
    }
}