using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Postscript.Interpreter.Values.Execution;

internal interface IExecutionSelector
{
    IExecutePostscript Literal{ get; }
    IExecutePostscript Executable { get; }
}

[StaticSingleton()]
public sealed partial class AlwaysLiteralSelector: IExecutionSelector
{
    public IExecutePostscript Literal => PostscriptBuiltInOperations.PushArgument;
    public IExecutePostscript Executable => PostscriptBuiltInOperations.PushArgument;
}

