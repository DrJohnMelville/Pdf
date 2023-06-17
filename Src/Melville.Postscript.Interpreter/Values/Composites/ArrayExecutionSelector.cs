using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

[StaticSingleton]
internal sealed partial class ArrayExecutionSelector : IExecutionSelector
{
    public IExecutePostscript Literal => PostscriptBuiltInOperations.PushArgument;
    public IExecutePostscript Executable => ArrayExecutor.Instance;
}