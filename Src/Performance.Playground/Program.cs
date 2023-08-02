using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Performance.Playground.Rendering;
 
#pragma warning disable CS0162

if (true)
{
    Console.WriteLine("Begin");
    await new ImageExtraction().ExtractImagesAsync();
    Console.WriteLine("done");
}
else
{
    BenchmarkRunner.Run<PageRendering>();
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