using Melville.Postscript.Interpreter.Values.Numbers;
using NetArchTest.Rules;
using Xunit;

namespace Melville.Pdf.DataModelTests.Architecture;

public class PostscriptInterpreterArchitecture
{
    private static Types AllTypes => Types.InAssembly(typeof(PostscriptInteger).Assembly);
    [Fact]
    public void CabinExternalDependencies() =>
        AllTypes
            .That()
            .ResideInNamespace("Melville.Postscript.Interpreter.Values")
            .Or().ResideInNamespace("Melville.Postscript.Interpreter.Tokenizers")
            .Or().ResideInNamespace("Melville.Postscript.Interpreter.InterpreterState")
            .Should()
            .NotHaveDependencyOn("Melville.Postscript.FunctionalLibrary")
            .ShouldSucceed();
    
}