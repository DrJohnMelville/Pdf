using Melville.Parsing.VariableBitEncoding;

namespace Melville.CCITT;

internal ref struct BitTarget
{
    private readonly Span<byte> target;
    private Span<byte> unwritten;
    private readonly BitWritingBuffer writer;
    public int BytesWritten => target.Length - unwritten.Length;

    public BitTarget(Span<byte> target, BitWritingBuffer writer)
    {
        this.target = target;
        unwritten = target;
        this.writer = writer;
    }

    public bool TryWriteBits(uint data, int bits)
    {
        writer.WriteBits(data, bits);
        return writer.TryWriteToMovingBuffer(ref unwritten);
    }

    public int BytesRemaining() => unwritten.Length;

    public void FinishWrite() 
    {
        writer.AdvanceToWholeByte();
        writer.TryWriteToMovingBuffer(ref unwritten);
    }

    public Span<byte> WrittenSpan() => target[..BytesWritten];
}