using System;
using System.Collections.Generic;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.Values;

internal partial class PostscriptLongDictionary :
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<IPostscriptComposite>,
    IPostscriptComposite
{
    [FromConstructor] private readonly Dictionary<PostscriptValue, PostscriptValue> items;

    public PostscriptLongDictionary() : this(new Dictionary<PostscriptValue, PostscriptValue>())
    {
    }

    string IPostscriptValueStrategy<string>.GetValue(in Int128 memento)
    {
        var ret = new StringBuilder();
        ret.AppendLine("<<");
        foreach (var pair in items)
        {
            ret.AppendLine($"    {pair.Key.ToString()}: {pair.Value.ToString()}");
        }
        ret.Append(">>");

        return ret.ToString();
    }

    IPostscriptComposite
        IPostscriptValueStrategy<IPostscriptComposite>.GetValue(in Int128 memento) => this;

    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result) =>
        items.TryGetValue(indexOrKey, out result);

    public void Put(in PostscriptValue indexOrKey, in PostscriptValue value) =>
        items[indexOrKey] = value;

    public int Length => items.Count;

    public PostscriptValue CopyFrom(PostscriptValue source, PostscriptValue target) => 
        throw new NotImplementedException("Dictionary Copying is not implemented yet");

    public ForAllCursor CreateForAllCursor()
    {
        throw new NotImplementedException("Dictionary forall is not implemented yet");
    }
}