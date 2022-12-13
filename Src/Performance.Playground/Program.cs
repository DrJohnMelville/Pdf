using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Performance.Playground.Encryption;
using Performance.Playground.ObjectModel;
using Performance.Playground.Rendering;
 
#pragma warning disable CS0162

if (true)
{
    Console.WriteLine("Begin");
    var pageRendering = new ThreadingBug();
    await pageRendering.ReadMulti();
    Console.WriteLine("done");
}
else
{
    BenchmarkRunner.Run<PageRendering>();
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
