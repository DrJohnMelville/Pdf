using System;
using System.Buffers;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.LinkedLists;

internal partial class StreamBackedBuffer: LinkedList<StreamBackedBuffer>
{
    public static MultiBufferStream Create(Stream source, int bufferSize = 4096)
    {
        var buffer = new StreamBackedBuffer(source);
        var linkedList = buffer.With(bufferSize);
        buffer.nextByteToRead = buffer.StartPosition;
        return new MultiBufferStream(linkedList, false);
    }

    [FromConstructor] private readonly Stream source;
    private LinkedListPosition nextByteToRead = default;

   

    protected override long BytesAvailableAfter(LinkedListPosition origin)
    {
        var desiredPosition = origin.GlobalPosition;

        while (true)
        {
            var delta = nextByteToRead.GlobalPosition - desiredPosition;
            if (delta > 0) return delta;
            (nextByteToRead, var done) = LoadFrom(source, nextByteToRead);
            if (done) return nextByteToRead.GlobalPosition - desiredPosition;
        }
    }

    protected override async ValueTask<long> BytesAvailableAfterAsync(
        LinkedListPosition origin)
    {
        var desiredPosition = origin.GlobalPosition;

        while (true)
        {
            var delta = nextByteToRead.GlobalPosition - desiredPosition;
            if (delta > 0) return delta;
            (nextByteToRead, var done) = await LoadFromAsync(source, nextByteToRead).CA();
            if (done) return nextByteToRead.GlobalPosition - desiredPosition;
        }
    }
}

internal class MultiBufferStreamList : LinkedList<MultiBufferStreamList>
{
    public static LinkedList WritableList(int blockSize) =>
        new MultiBufferStreamList().With(blockSize);
    public static LinkedList SingleItemList(ReadOnlyMemory<byte> source) =>
        new MultiBufferStreamList().With(source);

}

internal abstract class LinkedList<T> : LinkedList where T : LinkedList<T>
{

}

internal abstract class LinkedList
{
    public LinkedListPosition StartPosition { get; private set; }
    private LinkedListPosition endPosition;
    public LinkedListPosition EndPosition => endPosition;
    private int references;
    private int blockSize;

    protected internal LinkedList()
    {
    }

    public void AddReference() => references++;
    public void ReleaseReference() => references--;

    public LinkedList With(int blockSize)
    {
        this.blockSize = blockSize;
        LinkedListNode firstNode = CreateNewBlock();
        references = 1;
        StartPosition = endPosition = new LinkedListPosition(firstNode, 0);
        return this;
    }

    public LinkedList With(ReadOnlyMemory<byte> source)
    {
        LinkedListNode firstNode = LinkedListNode.Rent(source);
        blockSize = 0;
        references = 1;
        StartPosition = new LinkedListPosition(firstNode, 0);
        endPosition = new LinkedListPosition(firstNode, source.Length);
        return this;
    }


    private LinkedListNode CreateNewBlock() => LinkedListNode.Rent(blockSize);

    public ReadOnlySequence<byte> AsSequence() => SequenceAfter(StartPosition);

    private ReadOnlySequence<byte> SequenceAfter(LinkedListPosition start) =>
        start.SequenceTo(EndPosition);

    public (int, LinkedListPosition) Read(LinkedListPosition origin, Span<byte> buffer) => 
        ReadCore(origin, buffer, BytesAvailableAfter(origin));


    protected virtual long BytesAvailableAfter(LinkedListPosition origin) => 
        endPosition.GlobalPosition - origin.GlobalPosition;

    public async ValueTask<(int, LinkedListPosition)> ReadAsync(
        LinkedListPosition origin, Memory<byte> buffer)
    {
        var bytesToRead = await BytesAvailableAfterAsync(origin).CA();
        return ReadCore(origin, buffer.Span, bytesToRead);
    }

    protected virtual ValueTask<long> BytesAvailableAfterAsync(LinkedListPosition origin) => 
        new(endPosition.GlobalPosition - origin.GlobalPosition);

    private (int, LinkedListPosition) ReadCore(LinkedListPosition origin, Span<byte> buffer, long availableBytes)
    {
        var length = (int )Math.Min(availableBytes, buffer.Length);
        var seq = SequenceAfter(origin);
        var reader = new SequenceReader<byte>(seq);
        reader.TryCopyTo(buffer[..length]);
        return (length, seq.GetPosition(length));
    }

    public LinkedListPosition Write(
        LinkedListPosition currentPosition, ReadOnlySpan<byte> buffer) =>
        UpdateEndIfGreater(currentPosition.WriteTo(buffer, blockSize));

    public LinkedListPosition UpdateEndIfGreater(LinkedListPosition newPos)
    {
        if (newPos.GlobalPosition > endPosition.GlobalPosition)
            endPosition = newPos;
        return newPos;
    }

    public void Truncate(long value)
    {
        endPosition = AsSequence().GetPosition(value);
    }

    public long Length() => EndPosition.GlobalPosition;

    public void EnsureHasLocation(long value)
    {
        UpdateEndIfGreater(EndPosition.ExtendTo(value, blockSize));
    }

    protected (LinkedListPosition, bool atEnd) LoadFrom(
        Stream stream, LinkedListPosition readpos)
    {
        var ret = readpos.GetMoreBytes(stream, blockSize);
        UpdateEndIfGreater(ret.bufferEnd);
        return ret;
    }
    protected async ValueTask<(LinkedListPosition, bool)> LoadFromAsync(
        Stream stream, LinkedListPosition readpos)
    {
        var ret = await readpos.GetMoreBytesAsync(stream, blockSize).CA();
        UpdateEndIfGreater(ret.bufferEnd);
        return ret;
    }

}