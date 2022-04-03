using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Performance.Playground.Rendering;

namespace Performance.Playground
{
    class Program
    {
        static Task Main(string[] args)
        {
            #if DEBUG
            var summary = BenchmarkRunner.Run<FontRenderingPerf>();
           return Task.CompletedTask;
            #else
            return Timer.DoTime(() => new FontRenderingPerf().RenderWpf());
            #endif
        }
    }

    public static class Timer
    {
        public static async Task DoTime(Func<Task> item)
        {
            Console.WriteLine("Begin Test");
            var sw = new Stopwatch();
            sw.Start();
            await item();
            sw.Stop();
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds} ms.");
        }
        public static Task DoTime(Action item)
        {
            Console.WriteLine("Begin Test");
            var sw = new Stopwatch();
            sw.Start();
            item();
            sw.Stop();
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds} ms.");
            return Task.CompletedTask;
        }
    }
}