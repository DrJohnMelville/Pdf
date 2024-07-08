using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Performance.Playground.Rendering;
 
#pragma warning disable CS0162

switch (2)
{
    case 0:
        Console.WriteLine("Begin");
        await new FontRendering().Melville();
        Console.WriteLine("done");
        break;
    case 1:
        BenchmarkRunner.Run<FontRendering>();
        break;
    case 2:
        var ren = new FontRendering();
        for (int i = 0; i < 10_000; i++)
        {
            await ren.Melville();
        }
        break;
}

public static class Timer
    {
        public static async Task DoTimeAsync(Func<Task> item)
        {
            var sw = new Stopwatch(); 
            sw.Start();
            await item();
            sw.Stop();
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds} ms.");
        }
        public static Task DoTimeAsync(Action item)
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

#pragma warning restore Arch004 // Async method does not have name ending with Async