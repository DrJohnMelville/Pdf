using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Postscript.Interpreter.Values.Execution;

internal partial class ExecutionWrapper : BuiltInFunction
{
    [FromConstructor] private PostscriptValue innerValue;

    public override void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        innerValue.ExecutionStrategy.Execute(engine, innerValue);
    }
}