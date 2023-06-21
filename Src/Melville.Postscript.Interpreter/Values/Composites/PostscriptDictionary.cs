using System;
using System.Buffers;
using System.Collections;
using System.Text;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values.Composites;

internal abstract class PostscriptDictionary:
    PostscriptComposite,
    IPostscriptValueStrategy<PostscriptDictionary>
{

    protected override void StringRep(StringBuilder ret)
    {
        ret.AppendLine("<<");
        RenderTo(ret);
        ret.Append(">>");
    }

    protected abstract void RenderTo(StringBuilder sb);

    PostscriptDictionary IPostscriptValueStrategy<PostscriptDictionary>.GetValue(
        in Int128 memento) => this;

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