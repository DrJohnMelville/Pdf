using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.Streams;

internal class MultiBuffer
{
    private readonly MultiBufferNode head;
    public long Length { get; private set; }

    public MultiBufferPosition StartOfStream() => new(head, 0);

    public MultiBuffer(int blockLength) : this(new byte[blockLength])
    {
        Length = 0; //this was set incorrectly in the called constructor so fixed here
    }

    public MultiBuffer(byte[] firstBuffer)
    {
        head = new MultiBufferNode(firstBuffer, 0);
        Length = firstBuffer.Length;
    }


    public MultiBufferPosition Read(in MultiBufferPosition position, in Span<byte> buffer)
    {
        var readLen = (int)Math.Min(buffer.Length, Length - position.StreamPosition);
        return (readLen > 0) ? ReadFromMultiBlock(position, buffer[..readLen]) : position;
    }

    private MultiBufferPosition ReadFromMultiBlock(in MultiBufferPosition position, Span<byte> buffer)
    {
        MultiBufferNode block = position.Node;
        var blockOffset = (int)(position.StreamPosition - block.InitialPosition);
        int bytesRead = 0;
        while (true)
        {
            bytesRead += CopyBytes(block.Data.AsSpan(blockOffset), buffer.Slice(bytesRead));
            if (bytesRead >= buffer.Length) break;
            block = block.TryNextNode();
            blockOffset = 0;
        }

        return new MultiBufferPosition(block, position.StreamPosition + buffer.Length);
    }



    public MultiBufferPosition Write(in MultiBufferPosition position, in ReadOnlySpan<byte> buffer)
    {
        SetLength(Math.Max(Length, position.StreamPosition + buffer.Length));
        return WriteToMultiBlock(position, buffer);
    }

    private MultiBufferPosition WriteToMultiBlock(in MultiBufferPosition position, ReadOnlySpan<byte> buffer)
    {
        var block = position.Node;
        var blockOffset = (int)(position.StreamPosition - block.InitialPosition);
        int bytesWritten = 0;
        while (true)
        {
            bytesWritten += CopyBytes(buffer.Slice(bytesWritten), block.Data.AsSpan(blockOffset));
            if (bytesWritten >= buffer.Length) break;
            block = block.ForceNextNode();
            blockOffset = 0;
        }

        return new MultiBufferPosition(block, position.StreamPosition + buffer.Length);
    }

    public bool ExtendStreamFrom(Stream source) => 
        TryExtendLength(source.Read(BufferToExpandInto().Span));

    private bool TryExtendLength(int bytesRead)
    {
        Length += bytesRead;
        return bytesRead > 0;
    }

    private Memory<byte> BufferToExpandInto()
    {
        var node = GetLastNode();
        var spaceLeft = node.EndPosition - Length;
        return spaceLeft < 1 ? 
            node.ForceNextNode().Data : 
            node.Data.AsMemory()[^(int)spaceLeft..];
    }

    public async ValueTask<bool> ExtendFromAsync(
        Stream innerStream, CancellationToken cancellationToken) =>
        TryExtendLength(
            await innerStream.ReadAsync(BufferToExpandInto(), cancellationToken).CA());

    private MultiBufferNode GetLastNode() => head.FindPosition(Length).Node;

    private int CopyBytes(in ReadOnlySpan<byte> source, in Span<byte> destination)
    {
        var readLen = Math.Min(source.Length, destination.Length);
        source.Slice(0, readLen).CopyTo(destination);
        return readLen;
    }

    public void SetLength(long value)
    {
        Length = value;
    }

    public MultiBufferPosition FindPosition(long position)
    {
        if (position < 0 || position > Length)
            throw new ArgumentException("Invalid seek operation");
        return head.FindPosition(position);
    }

}