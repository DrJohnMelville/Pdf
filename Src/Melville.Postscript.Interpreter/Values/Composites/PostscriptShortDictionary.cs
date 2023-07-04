using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Values.Composites;



internal partial class PostscriptShortDictionary : PostscriptDictionary
{
    private PostscriptValue[] items;
    private int length;

    public PostscriptShortDictionary(int size)
    {
        items = new PostscriptValue[size * 2];
        length = 0;
    }

    public PostscriptShortDictionary(in ReadOnlySpan<PostscriptValue> keysAndValues): 
        this(keysAndValues.Length / 2)
    {
        if (keysAndValues.Length % 2 != 0) throw new InvalidDataException(
            "A short dictionary must have an even number of elements");
        keysAndValues.CopyTo(items.AsSpan());
        length = keysAndValues.Length / 2;
    }

    protected override void RenderTo(StringBuilder sb)
    {
        for (int i = 0; i < length*2; i += 2)
        {
            sb.AppendLine($"    {items[i].ToString()}: {items[i + 1].ToString()}");
        }
    }

    public override bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result)
    {
        var index = TryFindLocation(indexOrKey);
        result = index >= 0 ? items[index + 1] : default;
        return index >= 0;
    }

    private int TryFindLocation(scoped in PostscriptValue key)
    {
        // fast leg -- assumes no keys as values
        var firstIndex = items.AsSpan(0, length*2).IndexOf(key);
        if (firstIndex < 0) return -1;
        if (firstIndex % 2 == 0) return firstIndex;
        
        // slow leg
        for (int i = 0; i < length * 2; i += 2)
        {
            if (items[i].Equals(key))
            {
                return i;
            }
        }
        return -1;
    }

    private int NextWriteLocation(in PostscriptValue key)
    {
        var index = TryFindLocation(key);
        if (index >= 0) return index;
        TryExpandArray();
        return 2*(length++);
    }
    private void TryExpandArray()
    {
        if (length == 0)
        {
            items = new PostscriptValue[4];
            return;
        }
        if (length * 2 >= items.Length)
        {
            Array.Resize(ref items, items.Length*2);
        }
    }

    public override void Put(in PostscriptValue indexOrKey, in PostscriptValue value)
    {
        int pos = NextWriteLocation(indexOrKey);
        items[pos] = indexOrKey;
        items[pos+1] = value;
    }

    public override int Length => length;
    public override int MaxLength => items.Length / 2;

    public override void Undefine(PostscriptValue key)
    {
        var index = TryFindLocation(key);
        if (index < 0) return;
        var activeGroup = items.AsSpan(0, 2 * length);
        length--;
        if (activeGroup.Length - 2 > index )
        {
            activeGroup[(index + 2)..].CopyTo(activeGroup[index..]);
        }
    }

    public override ForAllCursor CreateForAllCursor() =>
        new(items.AsMemory(0, length * 2), 2);
}