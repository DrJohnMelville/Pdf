using Melville.Parsing.VariableBitEncoding;

namespace Melville.CCITT;

public ref struct BitTarget
{
    private readonly Span<byte> target;
    private readonly BitWriter writer;
    public int BytesWritten { get; set; }

    public BitTarget(Span<byte> target, BitWriter writer)
    {
        this.target = target;
        this.writer = writer;
        BytesWritten = 0;
    }

    public bool TryWriteBits(uint data, int bits)
    {
        if (BytesWritten + BytesNeeded(bits) >= target.Length) return false;
        BytesWritten += writer.WriteBits(data, bits, UnwrittenSpan());
        return true;
    }

    private Span<byte> UnwrittenSpan() => target[BytesWritten..];

    private int BytesNeeded(int bits) => (7+bits)/8;
    public int BytesRemaining() => target.Length - BytesWritten;

    public void FinishWrite() => BytesWritten += writer.FinishWrite(UnwrittenSpan());

    public Span<byte> WrittenSpan() => target[..BytesWritten];
}