using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Performance.Playground;
using Performance.Playground.Rendering;
 
#pragma warning disable CS0162

switch (0)
{
    case 0:
        Console.WriteLine("Begin");
        await new Bugs().ReadAllImages();
        Console.WriteLine("done");
        Console.ReadLine();
        break;
    case 1:
        BenchmarkRunner.Run<PageRendering>();
        break;
    case 2:
        for (int i = 0; i < 1_000; i++)
        {
            new BitmapWriting().Generic();
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

#pragma warning restore Arch004 // FillFromAsync method does not have name ending with FillFromAsync