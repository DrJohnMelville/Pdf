using Melville.INPC;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

[StaticSingleton()]
internal sealed partial class NameExecutionSelector : IExecutionSelector
{
    public IExecutePostscript Literal => PushLiteralName.Instance;
    public IExecutePostscript Executable => PostscriptBuiltInOperations.ExecuteFromDictionary;

    [StaticSingleton()]
    internal sealed partial class PushLiteralName : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value) => engine.OperandStack.Push(value);
        public override bool IsExecutable => false;
        public override string WrapTextDisplay(string text) => 
            "/" + base.WrapTextDisplay(text);
    }
}