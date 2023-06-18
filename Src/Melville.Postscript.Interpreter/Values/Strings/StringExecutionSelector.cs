using Melville.INPC;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

[StaticSingleton()]
public sealed partial class StringExecutionSelector: IExecutionSelector
{
    public IExecutePostscript Literal => PushLiteralString.Instance;
    public IExecutePostscript Executable => StringExecutor.Instance;

    [StaticSingleton()]
    internal sealed partial class PushLiteralString : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value) => engine.Push(value);
        public override string WrapTextDisplay(string text) => "(" + base.WrapTextDisplay(text)+")";
        public override bool IsExecutable => false;
    }
}