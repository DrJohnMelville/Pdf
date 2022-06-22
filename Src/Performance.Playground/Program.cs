using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Performance.Playground.Rendering;

await new JbigParsing().ParseJBig();
Console.WriteLine("done");
//BenchmarkRunner.Run<JbigParsing >();

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
