using System.Threading.Tasks;
using Melville.AsyncAnalyzer;
using Melville.AsyncCodeFix;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;


namespace Melville.ArchitectureAnalyzer.Test.AsyncDetectorTest
{
    public class AsyncCodeFixTest
    {
        [Test]
        [Arguments("public void XAsync() {}")]
        [Arguments("public async void XAsync() {}")]
        [Arguments("public int XAsync() => 1;")]
        [Arguments("public int XAsync => 1;")]
        public Task RemoveAsync(string code) => RunFixTest(
            code.Replace("XAsync", "{|#0:XAsync|}"), 
            code.Replace("XAsync", "X"), AsyncDiagnostics.UnneededAsync, "XAsync");

        [Test]
        [Arguments("public async Task X() {}")]
        [Arguments("public async ValueTask X() {}")]
        [Arguments("public async Task<int> X() => 1;")]
        [Arguments("public async ValueTask<int> X() => 1;")]
        [Arguments("public Task X() => Task.CompletedTask;")]
        [Arguments("public ValueTask X() => ValueTask.CompletedTask;")]
        [Arguments("public Task<int> X() => Task.FromResult(1);")]
        [Arguments("public ValueTask<int> X() => ValueTask.FromResult(1);")]
        [Arguments("public ValueTask<int> X => ValueTask.FromResult(1);")]
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