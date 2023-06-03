using System;
using System.Text;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

internal interface IPostscriptComposite
{
    PostscriptValue Get(in PostscriptValue indexOrKey);
}

internal partial class PostscriptArray : 
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<IPostscriptComposite>,
    IPostscriptComposite
{
    public static readonly PostscriptArray Empty = new(Memory<PostscriptValue>.Empty);

    [FromConstructor] private readonly Memory<PostscriptValue> values;

    public string GetValue(in Int128 memento)
    {
        var sb = new StringBuilder();
        sb.Append("[");
        foreach (var value in values.Span)
        {
            if (sb.Length > 1) sb.Append(", ");
            sb.Append(value.Get<string>());
        }
        sb.Append("]");
        return sb.ToString();
    }

    IPostscriptComposite
        IPostscriptValueStrategy<IPostscriptComposite>.GetValue(in Int128 memento) => this;

    public PostscriptValue Get(in PostscriptValue indexOrKey) => values.Span[indexOrKey.Get<int>()];
}