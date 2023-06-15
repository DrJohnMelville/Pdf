using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

#warning -- eventually this needs to implement IPdfComposite, and need to promote short strings to long strings on dup

internal sealed partial class PostscriptLongString: PostscriptString
{
    [FromConstructor]private readonly Memory<byte> value;

    public override ReadOnlySpan<byte> GetBytes(in Int128 memento, in Span<byte> scratch) =>
        value.Span;

    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.AddBytes(value.Span);
        return hc.ToHashCode();
    }
}