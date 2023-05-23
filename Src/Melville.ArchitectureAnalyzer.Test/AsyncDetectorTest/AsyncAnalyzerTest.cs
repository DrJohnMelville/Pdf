using System.Threading.Tasks;
using ArchitectureAnalyzer.Analyzer;
using Melville.AsyncAnalyzer;
using Microsoft;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.AsyncDetectorTest;

public class AsyncAnalyzerTest
{
    private static Task RunTest(string code, params DiagnosticResult[] diagnostics)
    {
        var tester =
            new CSharpAnalyzerTest<AsyncAnalyzerClass, XUnitVerifier>()
            {
                TestState = { 
                    Sources = {$$"""
                        using System.Threading.Tasks;
                        namespace Melville.NS;

                        public class Class 
                        {
                            {{code}}
                        }
                        """ }},
                ReferenceAssemblies = ReferenceAssemblies.Net.Net60
            };
        tester.TestState.ExpectedDiagnostics.AddRange(diagnostics);
        
        return tester.RunAsync();
    }

    [Fact]
    public Task AllowedNonAsyncMethod() => RunTest("public int X() => 1;");

    [Fact]
    public Task DisallowedNonAsyncMethod() =>
        RunTest("public int {|#0:XAsync|}() => 1;", 
            new DiagnosticResult(AsyncDiagnostics.UnneededAsync)
                .WithLocation(0)
                .WithArguments("XAsync"));

    [Theory]
    [InlineData("public async Task MethodAsync(){}")]
    [InlineData("public async Task<int> MethodAsync() => 1;")]
    [InlineData("public async ValueTask MethodAsync(){}")]
    [InlineData("public async ValueTask<int> MethodAsync() => 1;")]
    [InlineData("public Task MethodAsync() => Task.CompletedTask;")]
    [InlineData("public Task<int> MethodAsync() => Task.FromResult(1);")]
    [InlineData("public ValueTask MethodAsync() => ValueTask.CompletedTask;")]
    [InlineData("public ValueTask<int> MethodAsync() => new(1);")]
    public Task HasProperAsyncName(string code) => RunTest(code);

    [Theory]
    [InlineData("public async Task {|#0:Method|}(){}")]
    [InlineData("public async Task<int> {|#0:Method|}() => 1;")]
    [InlineData("public async ValueTask {|#0:Method|}(){}")]
    [InlineData("public async ValueTask<int> {|#0:Method|}() => 1;")]
    [InlineData("public Task {|#0:Method|}() => Task.CompletedTask;")]
    [InlineData("public Task<int> {|#0:Method|}() => Task.FromResult(1);")]
    [InlineData("public ValueTask {|#0:Method|}() => ValueTask.CompletedTask;")]
    [InlineData("public ValueTask<int> {|#0:Method|}() => new(1);")]
    public Task LacksAsyncName(string code) => RunTest(code,
        new DiagnosticResult(AsyncDiagnostics.AsyncNeeded)
            .WithLocation(0)
            .WithArguments("Method")
            
        );
}