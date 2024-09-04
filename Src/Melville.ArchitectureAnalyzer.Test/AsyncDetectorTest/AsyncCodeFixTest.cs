using System.Threading.Tasks;
using Melville.AsyncAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.AsyncDetectorTest
{
    public class AsyncCodeFixTest
    {
        [Theory]
        [InlineData("public void XAsync() {}")]
        [InlineData("public async void XAsync() {}")]
        [InlineData("public int XAsync() => 1;")]
        [InlineData("public int XAsync => 1;")]
        public Task RemoveAsync(string code) => RunFixTest(
            code.Replace("XAsync", "{|#0:XAsync|}"), 
            code.Replace("XAsync", "X"), AsyncDiagnostics.UnneededAsync, "XAsync");

        [Theory]
        [InlineData("public async Task X() {}")]
        [InlineData("public async ValueTask X() {}")]
        [InlineData("public async Task<int> X() => 1;")]
        [InlineData("public async ValueTask<int> X() => 1;")]
        [InlineData("public Task X() => Task.CompletedTask;")]
        [InlineData("public ValueTask X() => ValueTask.CompletedTask;")]
        [InlineData("public Task<int> X() => Task.FromResult(1);")]
        [InlineData("public ValueTask<int> X() => ValueTask.FromResult(1);")]
        [InlineData("public ValueTask<int> X => ValueTask.FromResult(1);")]
        public Task AddAsync(string code) => RunFixTest(
            code.Replace("X", "{|#0:X|}"), 
            code.Replace("X", "XAsync"), AsyncDiagnostics.AsyncNeeded, "X");

        private Task RunFixTest(string code, string fixedCode, DiagnosticDescriptor expectedDiagnostic,
            string offendingSymbol)
        {
            var fixTest =
                new CSharpCodeFixTest<AsyncAnalyzerClass, AsyncCodeFixProvider, DefaultVerifier>()
                {
                    TestState =
                    {
                        Sources = { WrapAsyncTestCase.Wrap(code) },
                        ReferenceAssemblies = ReferenceAssemblies.Net.Net60
                    },
                    FixedState= 
                    {
                        Sources = { WrapAsyncTestCase.Wrap(fixedCode) },
                        ReferenceAssemblies = ReferenceAssemblies.Net.Net60
                    },
                };

            fixTest.TestState.ExpectedDiagnostics.Add(
                new DiagnosticResult(expectedDiagnostic)
                    .WithLocation(0).WithArguments(offendingSymbol));

            return fixTest.RunAsync();
        }
    }
}