using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
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

    internal MultiBufferStream(LinkedList data, 
        bool canRead, bool canWrite, bool canSeek): 
        base(canRead, canWrite, canSeek)
    { 
        this.data = data;
        data.AddReference();
        currentPosition = data.StartPosition;
    }
    
    /// <summary>
    /// Create a MultDufferStream
    /// </summary>
    /// <param name="blockLength">The default block length when the stream creates blocks.</param>
    public MultiBufferStream(int blockLength = 4096): this(
        MultiBufferStreamList.WritableList(blockLength), false, true, true)
    {
    }

    /// <summary>
    /// Create a readonly multibufferstream that contains the given data
    /// </summary>
    /// <param name="firstBuffer">Make a multibufferStream with an initial buffer</param>
    public MultiBufferStream(ReadOnlyMemory<byte> firstBuffer) : 
        this(MultiBufferStreamList.SingleItemList(firstBuffer), true, false, true)
    {
    }


    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        Debug.Assert(CanRead);
        (var ret, currentPosition) = data.Read(currentPosition, buffer);
        return ret;
    }


    /// <inheritdoc />
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        Debug.Assert(CanRead);
        (var ret, currentPosition) = 
            await data.ReadAsync(currentPosition, buffer).CA();
        return ret;
    }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (!CanWrite)
            throw new NotSupportedException("This stream is read only");
        
        currentPosition = data.Write(currentPosition, buffer);
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        Debug.Assert(CanSeek);
        return Position = offset + this.SeekOriginLocation(origin);
    }


    /// <inheritdoc />
    public override void SetLength(long value) => 
      data.Truncate(value);

    /// <inheritdoc />
    public override long Length => data.Length();

    /// <inheritdoc />
    public override long Position
    {
        get => currentPosition.GlobalPosition;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            data.EnsureHasLocation(value);
            currentPosition = data.AsSequence().GetPosition(value);
        }
    }

    /// <inheritdoc />
    Stream IMultiplexSource.ReadFrom(long position)
    {
        var ret = new MultiBufferStream(data, true, false, true);
        if (position > 0) ret.Seek(position, SeekOrigin.Begin);
        return ret;
    }

    IByteSource IMultiplexSource.ReadPipeFrom(long position, long startingPosition)
    {
        var ret = new LinkedListByteSource(data);
        if (position > 0)
        {
            var initial = data.PositionAt(position);
            ret.AdvanceTo(initial);
        }

        return ret.WithCurrentPosition(startingPosition);
    }
}