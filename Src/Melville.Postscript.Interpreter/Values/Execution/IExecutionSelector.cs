
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values.Execution;

internal interface IExecutionSelector
{
    IExecutePostscript Literal{ get; }
    IExecutePostscript Executable { get; }
}

[StaticSingleton()]
internal sealed partial class AlwaysLiteralSelector: IExecutionSelector
{
    public IExecutePostscript Literal => PostscriptBuiltInOperations.PushArgument;
    public IExecutePostscript Executable => PostscriptBuiltInOperations.PushArgument;
}

