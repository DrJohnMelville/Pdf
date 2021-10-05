using BenchmarkDotNet.Running;
using Performance.Playground.Encryption;
using Performance.Playground.ObjectModel;
using Performance.Playground.Writers;

namespace Performance.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<StringCreation>();
        }
    }
}