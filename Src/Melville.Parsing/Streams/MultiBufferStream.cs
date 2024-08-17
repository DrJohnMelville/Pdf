using System.Diagnostics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.LinkedLists;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.Streams;

#warning -- consider making this an IMultiplexFactorySource so that I can optimize the unit tests 
internal class MultiBufferStream : DefaultBaseStream
{
    private LinkedList data;
    private LinkedListPosition currentPosition;
    private CountedSourceTicket ticket;
    
    internal MultiBufferStream(LinkedList data,
        bool canRead, bool canWrite, bool canSeek, CountedSourceTicket ticket) :
        base(canRead, canWrite, canSeek)
    {
        this.data = data;
        this.ticket = ticket;
        currentPosition = data.StartPosition;
    }

    public override int Read(Span<byte> buffer)
    {
        Debug.Assert(CanRead);
        (var ret, currentPosition) = data.Read(currentPosition, buffer);
        return ret;
    }


    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        Debug.Assert(CanRead);
        (var ret, currentPosition) =
            await data.ReadAsync(currentPosition, buffer).CA();
        return ret;
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (!CanWrite)
            throw new NotSupportedException("This stream is read only");

        currentPosition = data.Write(currentPosition, buffer);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        Debug.Assert(CanSeek);
        return Position = offset + this.SeekOriginLocation(origin);
    }


    public override void SetLength(long value) =>
        data.Truncate(value);

    public override long Length => data.Length;

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

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ticket.TryRelease();
    }
}