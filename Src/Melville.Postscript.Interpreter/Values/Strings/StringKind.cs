using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// Strategy class for different kinds of postscript strings and names
/// </summary>
public class StringKind
{
    /// <summary>
    /// Represents a postscript string.
    /// </summary>
    public static readonly StringKind String = new (StringExecutionSelector.Instance, false);
    /// <summary>
    /// Represents a postscript executable name
    /// </summary>
    public static readonly StringKind Name = new(NameExecutionSelector.Instance, true);
    /// <summary>
    /// represents a postscript literal name
    /// </summary>
    public static readonly StringKind LiteralName = new(NameExecutionSelector.Instance, false);

    internal IExecutePostscript DefaultAction { get; }
    internal IExecutionSelector ExecutionSelector { get; }
    internal PostscriptShortString ShortStringStraegy { get; }

    private StringKind(IExecutionSelector selector, bool parseAsExecutable)
    {
        ExecutionSelector = selector;
        DefaultAction = parseAsExecutable ? ExecutionSelector.Executable : ExecutionSelector.Literal;
        ShortStringStraegy = new(this);
    }
};