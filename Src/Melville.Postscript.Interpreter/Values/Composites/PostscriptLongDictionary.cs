using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Composites;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Melville.Postscript.Interpreter.Values;

internal partial class PostscriptLongDictionary :PostscriptDictionary
{
    [FromConstructor] private readonly Dictionary<PostscriptValue, PostscriptValue> items;

    public PostscriptLongDictionary() : this(new Dictionary<PostscriptValue, PostscriptValue>())
    {
    }

    protected override void RenderTo(StringBuilder sb)
    {
        foreach (var pair in items)
        {
            sb.AppendLine(CultureInfo.InvariantCulture,$"    {pair.Key}: {pair.Value}");
        }
    }

    public override bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result) =>
        items.TryGetValue(indexOrKey, out result);

    public override void Put(in PostscriptValue indexOrKey, in PostscriptValue value) =>
        items[indexOrKey] = value;

    public override int Length => items.Count;
    public override int MaxLength => Length;

    public override void Undefine(PostscriptValue key) => items.Remove(key);

    public override ForAllCursor CreateForAllCursor()
    {
        var temp = new PostscriptValue[items.Count *2];
        int pos = 0;
        foreach (var pair in items)
        {
            temp[pos++] = pair.Key;
            temp[pos++] = pair.Value;
        }

        return new(temp.AsMemory(), 2);
    }
}