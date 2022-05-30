using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

/// <summary>
/// In this struct SCREAMINGCAPS names are the names given to operations in Annex E of the ITU T88 spec
/// </summary>

public ref struct TwoByteBuffer
{
    public byte B { get; private set; }
    public byte B1 { get; private set; }

    public void Initialize(ref SequenceReader<byte> source)
    {
        B = GetByte(ref source);
        B1 = GetByte(ref source);
    }

    private byte GetByte(ref SequenceReader<byte> source)
    {
        source.TryRead(out var ret);
        return ret;
    }

    public void Advance(ref SequenceReader<byte> source) => (B, B1) = (B1, GetByte(ref source));
}