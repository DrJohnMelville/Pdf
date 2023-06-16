using System.Linq;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

internal class StringKind
{
    public static readonly StringKind String = new ("({0})", PostscriptBuiltInOperations.PushArgument);
    public static readonly StringKind Name = new("{0}", PostscriptBuiltInOperations.ExecuteFromDictionary);
    public static readonly StringKind LiteralName = new("/{0}", PostscriptBuiltInOperations.PushArgument);

    private readonly string formatString;
    public IExecutePostscript Action { get; }
    public PostscriptShortString ShortStringStraegy { get; }

    private StringKind(string formatString, IExecutePostscript action)
    {
        this.formatString = formatString;
        Action = action;
        ShortStringStraegy = new(this);
    }

    public string ToDisplay(string input) => string.Format(formatString, input);


};