using System;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

public readonly struct ContentStreamValueUnion
{
    public ContentStreamValueType Type { get; }
    public object? Object { get; }
    public double Floating { get; }
    public long Integer { get; }
    public Memory<byte> Bytes { get; }

    public ContentStreamValueUnion(object obj)
    {
        Type = ContentStreamValueType.Object;
        this.Object = obj;
        Floating = 0;
        Integer = 0;
        Bytes = Memory<byte>.Empty;
    }
    public ContentStreamValueUnion(in Memory<byte> memory)
    {
        Type = ContentStreamValueType.Memory;
        Object = null;
        Floating = 0;
        Integer = 0;
        Bytes = memory;
    }
    public ContentStreamValueUnion(double floating, long longValue)
    {
        Type = ContentStreamValueType.Number;
        Object = null;
        Floating = floating;
        Integer = longValue;
        Bytes = Memory<byte>.Empty;
    }

    public PdfName AsName() => (PdfName)(Object??
                               throw new PdfParseException("Should be a PdfName"));
}