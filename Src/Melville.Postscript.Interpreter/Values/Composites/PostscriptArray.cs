using System;
using System.Collections.Generic;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Postscript.Interpreter.Values;

internal partial class PostscriptArray : 
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<IPostscriptComposite>,
    IPostscriptComposite
{
    public static readonly PostscriptArray Empty = new(Memory<PostscriptValue>.Empty);

    [FromConstructor] private readonly Memory<PostscriptValue> values;

    public PostscriptArray(int length): this(new PostscriptValue[length].AsMemory()) {}

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

    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result) =>
        indexOrKey.TryGet(out int index) && index < values.Length
            ? values.Span[index].AsTrueValue(out result)
            : default(PostscriptValue).AsFalseValue(out result);

    public void Add(in PostscriptValue indexOrKey, in PostscriptValue value) =>
        values.Span[indexOrKey.Get<int>()] = value;
}