using System;
using System.Buffers;
using System.Text;

namespace Melville.Pdf.LowLevel.Model.ShortStrings;

internal interface IShortString
{
    int ComputeHashCode();
    int Length();
    bool SameAs(in ReadOnlySpan<byte> other);
    string ValueAsString();
    void Fill(Span<byte> span);
}

internal readonly struct ShortString<T> : IShortString where T : IPackedBytes
{
    private readonly T data;

    public ShortString(T data)
    {
        this.data = data;
    }

    public int ComputeHashCode()
    {
        var hash = new FnvComputer();
        data.AddToHash(ref hash);
        return hash.HashAsInt();
    }

    public int Length() => data.Length();

    public bool SameAs(in ReadOnlySpan<byte> other) =>
        data.SameAs(other);

    public string ValueAsString()
    {
        Span<byte> chars = stackalloc byte[data.Length()];
        data.Fill(chars);
        return Encoding.UTF8.GetString(chars);
    }

    public void Fill(Span<byte> span) => data.Fill(span);
}