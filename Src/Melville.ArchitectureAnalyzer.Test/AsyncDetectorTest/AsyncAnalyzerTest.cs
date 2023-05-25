using System.Threading.Tasks;
using ArchitectureAnalyzer.Analyzer;
using Melville.AsyncAnalyzer;
using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.AsyncDetectorTest;

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

public class AsyncCodeFixTest
{
    [Theory]
    [InlineData("public void XAsync() {}")]
    [InlineData("public async void XAsync() {}")]
    [InlineData("public int XAsync() => 1;")]
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
    public Task AddAsync(string code) => RunFixTest(
        code.Replace("X", "{|#0:X|}"), 
        code.Replace("X", "XAsync"), AsyncDiagnostics.AsyncNeeded, "X");

    private Task RunFixTest(string code, string fixedCode, DiagnosticDescriptor expectedDiagnostic,
        string offendingSymbol)
    {
        var fixTest =
            new CSharpCodeFixTest<AsyncAnalyzerClass, AsyncCodeFixProvider, XUnitVerifier>()
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

public class AsyncAnalyzerTest
{
    private static Task RunTest(string code, params DiagnosticResult[] diagnostics)
    {
        var tester =
            new CSharpAnalyzerTest<AsyncAnalyzerClass, XUnitVerifier>()
            {
                TestState = { 
                    Sources = {WrapAsyncTestCase.Wrap(code)},
                ReferenceAssemblies = ReferenceAssemblies.Net.Net60}
            };
        tester.TestState.ExpectedDiagnostics.AddRange(diagnostics);
        
        return tester.RunAsync();
    }

    [Fact]
    public Task AllowedNonAsyncMethod() => RunTest("public int X() => 1;");
    [Fact]
    public Task AllowedAsyncVoidMethod() => RunTest("public async void X() {}");

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