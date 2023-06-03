using System;
using System.Collections.Generic;
using System.Text;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

internal partial class PostscriptLongDictionary :
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<IPostscriptComposite>,
    IPostscriptComposite
{
    [FromConstructor] private readonly Dictionary<PostscriptValue, PostscriptValue> items;

    string IPostscriptValueStrategy<string>.GetValue(in Int128 memento)
    {
        var ret = new StringBuilder();
        ret.AppendLine("<<");
        foreach (var pair in items)
        {
            ret.AppendLine($"    {pair.Key.Get<string>()}: {pair.Value.Get<string>()}");
        }
        ret.Append(">>");

        return ret.ToString();
    }

    IPostscriptComposite
        IPostscriptValueStrategy<IPostscriptComposite>.GetValue(in Int128 memento) => this;

    public PostscriptValue Get(in PostscriptValue indexOrKey)
    {
        return items[indexOrKey];
    }
}