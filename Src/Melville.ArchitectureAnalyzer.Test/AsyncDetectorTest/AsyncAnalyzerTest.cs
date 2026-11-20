using System.Threading.Tasks;
using Melville.AsyncAnalyzer;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Melville.ArchitectureAnalyzer.Test.AsyncDetectorTest;

public class AsyncAnalyzerTest
{
    private static Task RunTest(string code, params DiagnosticResult[] diagnostics)
    {
        var tester =
            new CSharpAnalyzerTest<AsyncAnalyzerClass, DefaultVerifier>()
            {
                TestState = { 
                    Sources = {WrapAsyncTestCase.Wrap(code)},
                ReferenceAssemblies = ReferenceAssemblies.Net.Net60}
            };
        tester.TestState.ExpectedDiagnostics.AddRange(diagnostics);
        
        return tester.RunAsync();
    }


    [Test]
    [Arguments("public int X() => 1;")]
    [Arguments("public static Task Main() => null;")]
    [Arguments("public async void X() {}")]
    [Arguments("public Task<int> this[int index] => Task.FromResult(index);")]
    [Arguments("""
        public interface Int
        {
        #pragma warning disable Arch004
                    public Task Method();
        #pragma warning restore Arch004
        }

        public class Inner : Int
        {
            public Task Method() => null;
        }
        """)]
    [Arguments("""
        public class Int
        {
        #pragma warning disable Arch004
                    public virtual Task Method() => null!;
        #pragma warning restore Arch004
        }

        public class Inner : Int
        {
            public override Task Method() => null;
        }
        """)]
    public Task ProperlyLacksAsyncSuffix(string code) => RunTest(code);

    [Test]
    [Arguments("public async Task MethodAsync(){}")]
    [Arguments("public async Task<int> MethodAsync() => 1;")]
    [Arguments("public async ValueTask MethodAsync(){}")]
    [Arguments("public async ValueTask<int> MethodAsync() => 1;")]
    [Arguments("public Task MethodAsync() => Task.CompletedTask;")]
    [Arguments("public Task<int> MethodAsync() => Task.FromResult(1);")]
    [Arguments("public ValueTask MethodAsync() => ValueTask.CompletedTask;")]
    [Arguments("public ValueTask<int> MethodAsync() => new(1);")]
    public Task ProperlyHasAsyncSuffix(string code) => RunTest(code);

    [Test]
    [Arguments("public int {|#0:XAsync|}() => 1;")]
    [Arguments("public int {|#0:XAsync|} => 1;")]
    public Task ImproperlyHasAsyncSuffix(string code) =>
        RunTest(code, new DiagnosticResult(AsyncDiagnostics.UnneededAsync)
                .WithLocation(0)
                .WithArguments("XAsync"));

    [Test]
    [Arguments("public async Task {|#0:Method|}(){}")]
    [Arguments("public async Task<int> {|#0:Method|}() => 1;")]
    [Arguments("public async ValueTask {|#0:Method|}(){}")]
    [Arguments("public async ValueTask<int> {|#0:Method|}() => 1;")]
    [Arguments("public Task {|#0:Method|}() => Task.CompletedTask;")]
    [Arguments("public Task<int> {|#0:Method|}() => Task.FromResult(1);")]
    [Arguments("public ValueTask {|#0:Method|}() => ValueTask.CompletedTask;")]
    [Arguments("public ValueTask<int> {|#0:Method|}() => new(1);")]
    public Task ImproperlyLacksAsyncSuffix(string code) => RunTest(code,
        new DiagnosticResult(AsyncDiagnostics.AsyncNeeded)
            .WithLocation(0)
            .WithArguments("Method")
            
        );
}