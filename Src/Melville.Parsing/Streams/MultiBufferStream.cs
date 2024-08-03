using System.Buffers;
using System.Runtime.InteropServices;
using Melville.Parsing.LinkedLists;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.PipeReaders;
using Melville.Parsing.Streams.Bases;
using LinkedListPosition = Melville.Parsing.LinkedLists.LinkedListPosition;

namespace Melville.Parsing.Streams;

/// <summary>
/// This class acts like a memorystream, except that it uses a list of buffers instead of resizing the buffer.
/// </summary>
public class MultiBufferStream : DefaultBaseStream, IMultiplexSource
{
    private LinkedList data;
    private LinkedListPosition currentPosition;

    /// <summary>
    /// Create a MultDufferStream
    /// </summary>
    /// <param name="blockLength">The default block length when the stream creates blocks.</param>
    public MultiBufferStream(int blockLength = 4096): base(true, true, true)
    {
        data = LinkedList.WritableList(blockLength);
        currentPosition = data.StartPosition;
    }
    
    /// <summary>
    /// Create a readonly multibufferstream that contains the given data
    /// </summary>
    /// <param name="firstBuffer">Make a multibufferStream with an initial buffer</param>
    public MultiBufferStream(ReadOnlyMemory<byte> firstBuffer) : base(true, false, true)
    {
        data = LinkedList.SingleItemList(firstBuffer);
        currentPosition = data.StartPosition;
    }


    /// <inheritdoc />
    public override int Read(Span<byte> buffer) => data.Read(ref currentPosition, buffer);

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (!CanWrite)
            throw new NotSupportedException("This stream is read only");
        
        currentPosition = data.Write(currentPosition, buffer);
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) =>
        Position = offset + this.SeekOriginLocation(origin);


    /// <inheritdoc />
    public override void SetLength(long value) => 
      data.Truncate(value);

    /// <inheritdoc />
    public override long Length => data.Length();

    /// <inheritdoc />
    public override long Position
    {
        get => currentPosition.GlobalPosition;
        set => currentPosition =  data.AsSequence().GetPosition(value);
    }

    /// <summary>
    /// Create a reader that has its own unique position pointer into the buffer.
    /// </summary>
    public MultiBufferStream CreateReader() => new(data);

    private MultiBufferStream(LinkedList data): base(true, false, true)
    { 
        this.data = data;
        data.AddReference();
        currentPosition = data.StartPosition;
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