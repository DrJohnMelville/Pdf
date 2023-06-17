using System;
using System.Linq;
using Melville.INPC;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

internal class StringKind
{
    public static readonly StringKind String = new (StringExecutionSelector.Instance, false);
    public static readonly StringKind Name = new(NameExecutionSelector.Instance, true);
    public static readonly StringKind LiteralName = new(NameExecutionSelector.Instance, false);

    public IExecutePostscript DefaultAction { get; }
    public IExecutionSelector ExecutionSelector { get; }
    public PostscriptShortString ShortStringStraegy { get; }

    private StringKind(IExecutionSelector selector, bool parseAsExecutable)
    {
        ExecutionSelector = selector;
        DefaultAction = parseAsExecutable ? ExecutionSelector.Executable : ExecutionSelector.Literal;
        ShortStringStraegy = new(this);
    }
};

[StaticSingleton()]
internal sealed partial class NameExecutionSelector : IExecutionSelector
{
    public IExecutePostscript Literal => PushLiteralName.Instance;
    public IExecutePostscript Executable => PostscriptBuiltInOperations.ExecuteFromDictionary;

    [StaticSingleton()]
    internal sealed partial class PushLiteralName : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value) => engine.Push(value);
        public override bool IsExecutable => false;
        public override string WrapTextDisplay(string text) => "/" + base.WrapTextDisplay(text);
    }
}

[StaticSingleton()]
public sealed partial class StringExecutionSelector: IExecutionSelector
{
    public IExecutePostscript Literal => PushLiteralString.Instance;
    public IExecutePostscript Executable => throw new NotImplementedException("String execution");

    [StaticSingleton()]
    internal sealed partial class PushLiteralString : BuiltInFunction
    {
        public override void Execute(PostscriptEngine engine, in PostscriptValue value) => engine.Push(value);
        public override string WrapTextDisplay(string text) => "(" + base.WrapTextDisplay(text)+")";
        public override bool IsExecutable => false;
    }
}