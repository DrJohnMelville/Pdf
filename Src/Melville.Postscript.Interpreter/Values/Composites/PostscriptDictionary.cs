﻿using System;
using System.Buffers;
using System.Text;

namespace Melville.Postscript.Interpreter.Values.Composites;

/// <summary>
/// Represents a dictionary created by the postscript interpreter.
/// </summary>
public interface IPostscriptDictionary : IPostscriptComposite
{
    /// <summary>
    /// Remove item with a given key from the dictionary
    /// </summary>
    /// <param name="key"></param>
    public void Undefine(PostscriptValue key);

    /// <summary>
    /// The number of keys in the dictionary
    /// </summary>
    public int MaxLength { get; }
}

internal abstract class PostscriptDictionary:
    PostscriptComposite, IPostscriptDictionary
{

    protected override void StringRep(StringBuilder ret)
    {
        ret.AppendLine("<<");
        RenderTo(ret);
        ret.Append(">>");
    }

    protected abstract void RenderTo(StringBuilder sb);
    
    public abstract int MaxLength { get; }
    
    public abstract void Undefine(PostscriptValue key);

    public override PostscriptValue CopyFrom(
        PostscriptValue source, PostscriptValue target)
    {
        var src = source.Get<PostscriptDictionary>().CreateForAllCursor();
        var buffer = ArrayPool<PostscriptValue>.Shared.Rent(2);
        int position = 0;
        while (src.TryGetItr(buffer.AsSpan(0, 2), ref position))
        {
            Put(buffer[0], buffer[1]);
        }

        return target;
    }

}