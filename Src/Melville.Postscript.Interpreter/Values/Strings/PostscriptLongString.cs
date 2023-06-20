using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

#warning -- eventually this needs to implement IPdfComposite

internal sealed partial class PostscriptLongString: PostscriptString
{
    [FromConstructor]private readonly Memory<byte> value;

    internal override Span<byte> GetBytes(
        scoped in Int128 memento, scoped in Span<byte> scratch) =>
        value.Span;

    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.AddBytes(value.Span);
        return hc.ToHashCode();
    }


    protected override Memory<byte> ValueAsMemory(in Int128 memento) => value;
}