using System.Linq;
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