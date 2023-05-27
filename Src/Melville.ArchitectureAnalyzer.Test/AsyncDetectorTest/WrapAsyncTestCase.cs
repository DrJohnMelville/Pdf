namespace Melville.ArchitectureAnalyzer.Test.AsyncDetectorTest
{
    public static class WrapAsyncTestCase
    {
        public static string Wrap(string code) => $$"""
                        using System.Threading.Tasks;
                        namespace Melville.NS;

                        public class Class 
                        {
                            {{code}}
                        }
                        """;
    }
}