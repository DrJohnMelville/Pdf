using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.Values.Composites;

internal partial class PostscriptShortDictionary :
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<IPostscriptComposite>,
    IPostscriptComposite
{
    public static readonly PostscriptShortDictionary Empty = new(new List<PostscriptValue>());

    [FromConstructor] private readonly List<PostscriptValue> items;
    public PostscriptShortDictionary():this(new List<PostscriptValue>()){}

#if DEBUG
    partial void OnConstructed()
    {
        if (items.Count % 2 != 0) throw new InvalidDataException(
            "A short dictionary must have an even number of elements");

    }
#endif

    string IPostscriptValueStrategy<string>.GetValue(in Int128 memento)
    {
        var ret = new StringBuilder();
        ret.AppendLine("<<");
        for (int i = 0; i < items.Count; i += 2)
        {
            ret.AppendLine($"    {items[i].Get<string>()}: {items[i + 1].Get<string>()}");
        }
        ret.Append(">>");

        return ret.ToString();
    }

    IPostscriptComposite
        IPostscriptValueStrategy<IPostscriptComposite>.GetValue(in Int128 memento) => this;

    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result)
    {
        for (int i = 0; i < items.Count; i += 2)
        {
            if (items[i].Equals(indexOrKey))
            {
                result = items[i + 1];
                return true;
            }
        }
        result = default;
        return false;
    }

    public void Put(in PostscriptValue indexOrKey, in PostscriptValue value)
    {
        items.Add(indexOrKey);
        items.Add(value);
    }

    public int Length => items.Count / 2;

    public PostscriptValue CopyFrom(PostscriptValue pop) =>
        throw new NotImplementedException("Dictionary Copying is not implemented yet");

}