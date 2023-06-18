using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

#warning -- eventually this needs to implement IPdfComposite, and need to promote short strings to long strings on dup

internal sealed partial class PostscriptLongString: PostscriptString
{
    [FromConstructor]private readonly Memory<byte> value;

    protected override Span<byte> GetBytes(in Int128 memento, in Span<byte> scratch) =>
        value.Span;

    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.AddBytes(value.Span);
        return hc.ToHashCode();
    }


    protected override Memory<byte> ValueAsMemory(in Int128 memento) => value;
}