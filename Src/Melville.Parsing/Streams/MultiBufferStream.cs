using System.Diagnostics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.LinkedLists;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.Streams;

#warning -- consider making this an IMultiplexFactorySource so that I can optimize the unit tests 
internal class MultiBufferStream : ReadWriteStreamBase
{
    private LinkedList data = LinkedList.Empty;
    private LinkedListPosition currentPosition;
    private CountedSourceTicket ticket;
    private bool writable;

    internal static MultiBufferStream Create(LinkedList data, bool writable, CountedSourceTicket ticket)
    {
        var ret = ObjectPool<MultiBufferStream>.Shared.Rent();
        Debug.Assert(!ret.IsValid());
        ret.data = data;
        ret.ticket = ticket;
        ret.writable = writable;
        ret.currentPosition = data.StartPosition;
        return ret;
    }

    private bool IsValid() => data != LinkedList.Empty;

    protected override void Dispose(bool disposing)
    {
        if (!IsValid()) return;
        ticket.TryRelease();
        data = LinkedList.Empty;
        currentPosition = LinkedListPosition.NullPosition;
        base.Dispose(disposing);
        ObjectPool<MultiBufferStream>.Shared.Return(this);
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

    public override long Seek(long offset, SeekOrigin origin) => 
        Position = offset + this.SeekOriginLocation(origin);


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

    public override void Flush()
    {
    }

    public override bool CanRead => !writable;

    public override bool CanSeek => true;

    public override bool CanWrite => writable;
}