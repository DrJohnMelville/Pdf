using System.Buffers;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.PipeReaders;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.Streams;

/// <summary>
/// This class acts like a memorystream, except that it uses a list of buffers instead of resizing the buffer.
/// </summary>
public class MultiBufferStream : DefaultBaseStream, IMultiplexSource
{
    private LinkedListPosition startPosition;
    private LinkedListPosition endPosition;
    private LinkedListPosition currentPosition;
    private readonly int blockLength;

    /// <summary>
    /// Create a MultDufferStream
    /// </summary>
    /// <param name="blockLength">The default block length when the stream creates blocks.</param>
    public MultiBufferStream(int blockLength = 4096): base(true, true, true)
    {
        this.blockLength = blockLength;
        startPosition = endPosition = currentPosition = 
            new LinkedListPosition(RentNewBlock(), 0);
    }

    private LinkedListNode RentNewBlock() => 
        new LinkedListNode().With(new byte[blockLength], null);

    /// <summary>
    /// Create a multibufferstream that contains the given data
    /// </summary>
    /// <param name="firstBuffer"></param>
    public MultiBufferStream(byte[] firstBuffer) : base(true, false, true)
    {
        blockLength = 0;
        var node = new LinkedListNode().With(firstBuffer, null);
        startPosition = currentPosition =
            new LinkedListPosition(node, 0);
        endPosition = new LinkedListPosition(node, firstBuffer.Length);
    }

    private bool AtEndOfData() => currentPosition == endPosition;

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        var seq = currentPosition.SequenceTo(endPosition);
        var length = Math.Min(seq.Length, buffer.Length);
        var reader = new SequenceReader<byte>(seq);
        reader.TryCopyTo(buffer.Slice(0, (int)length));
        currentPosition = seq.GetPosition(length);
        return (int)length;
    }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (!CanWrite)
            throw new NotSupportedException("This stream is read only");

        if (currentPosition == endPosition)
        {
            currentPosition = endPosition = currentPosition.Append(buffer, blockLength);
        }
        else
        {
            currentPosition = 
                currentPosition.WriteTo(buffer, blockLength, ref endPosition);
        }
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) =>
        Position = offset + this.SeekOriginLocation(origin);


    /// <inheritdoc />
    public override void SetLength(long value) => 
      endPosition = GetPosition(value);

    private SequencePosition GetPosition(long value)
    {
        if (value < 0 || value > endPosition.GlobalPosition)
            throw new ArgumentOutOfRangeException(nameof(value), "Position is invalid");
        return startPosition.SequenceTo(endPosition).GetPosition(value);
    }

    /// <inheritdoc />
    public override long Length => endPosition.GlobalPosition;

    /// <inheritdoc />
    public override long Position
    {
        get => currentPosition.GlobalPosition;
        set => currentPosition =  GetPosition(value);
    }

    /// <summary>
    /// Create a reader that has its own unique position pointer into the buffer.
    /// </summary>
    public MultiBufferStream CreateReader() => new(startPosition, endPosition);

    private MultiBufferStream(
        LinkedListPosition startPosition, 
        LinkedListPosition endPosition): base(true, false, true)
    {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        currentPosition = startPosition;
        blockLength = 0;
    }

    /// <inheritdoc />
    Stream IMultiplexSource.ReadFrom(long position)
    {
        var ret = CreateReader();
        ret.Seek(position, SeekOrigin.Begin);
        return ret;
    }
#warning Implement IMultiplexSource.ReadPipeFrom
    // IByteSource IMultiplexSource.ReadPipeFrom(
    //     long position, long startingPosition = 0)
    // {
    //     return new SequenceByteReader(
    //         startPosition.SequenceTo(endPosition).Slice(position), startingPosition);
    // }
}