using System;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal ref struct SpanWriter
{
    private readonly Span<byte> backingStore;
    private int length;

    public SpanWriter(Span<byte> backingStore)
    {
        this.backingStore = backingStore;
    }
    
    public void Append(ReadOnlySpan<byte> source)
    {
        source.CopyTo(backingStore[length..]);
        length += source.Length;
    }

    public void DuplicateNTimes(int n)
    {
        ReadOnlySpan<byte> source = BuiltSpan();
        for (int i = 0; i < n; i++)
        {
            Append(source);
        }
    }

    public Span<byte> BuiltSpan() => backingStore[..length];
}