using BenchmarkDotNet.Running;
using Performance.Playground.Writers;

namespace Performance.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MemoryStreams>();
        }
    }
}