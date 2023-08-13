using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Strings;

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
    /// <summary>
    /// Shortstringstrategy containing this kind
    /// </summary>
    internal Strings.PostscriptShortString Strategy6Bit { get; }
    internal Strings.PostscriptShortString Strategy8Bit { get; }
    internal Strings.PostscriptShortString Strategy16Bytes { get; }

    private StringKind(IExecutionSelector selector, bool parseAsExecutable)
    {
        ExecutionSelector = selector;
        DefaultAction = parseAsExecutable ? ExecutionSelector.Executable : ExecutionSelector.Literal;
        Strategy6Bit = new PostscriptShortString6Bit(this);
        Strategy8Bit = new PostscriptShortString8Bit(this);
        Strategy16Bytes = new Postscript16ByteString8Bit(this);
    }
};