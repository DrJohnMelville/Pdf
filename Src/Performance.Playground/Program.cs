using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Melville.Parsing.AwaitConfiguration;
using Performance.Playground.Encryption;
using Performance.Playground.ObjectModel;
using Performance.Playground.Rendering;
using Performance.Playground.Writers;

namespace Performance.Playground
{
    class Program
    {
        static Task Main(string[] args)
        {
          //  var summary = BenchmarkRunner.Run<LoadCCITPerf>();
            //return Task.CompletedTask;
            return Timer.DoTime(() => new LoadCCITPerf().RenderPage());
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
    }
}