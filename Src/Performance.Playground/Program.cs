using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Performance.Playground.ObjectModel;
using Performance.Playground.Rendering;

#pragma warning disable CS0162

if (false)
{
    await new CcittParsing().RenderSkia();
    Console.WriteLine("done");
}
else
{
    BenchmarkRunner.Run<DictionaryVsList>();
}

public static class Timer
    {
        public static async Task DoTime(Func<Task> item)
        {
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
