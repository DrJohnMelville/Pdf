using Melville.Parsing.MultiplexSources;
using Melville.StreamInterfaces;
using System.Buffers;
using System.ComponentModel;

namespace Melville.Parsing.LinkedLists;

/// <summary>
/// This buffer can be written to and then read by multiple independent readers.
/// It maintains its state as a linked list of buffers.  It does not reallocate to
/// grow.
/// </summary>
public readonly struct MultiMemoryBuffer : IDisposable
{
    private readonly MultiBufferStreamList data;

    /// <summary>
    /// Create a new MultiMemoryBuffer
    /// </summary>
    public MultiMemoryBuffer()
    {
        data = new MultiBufferStreamList();
        data.With(4096);

    }

    /// <inheritdoc/>
    public void Dispose() => data.Dispose();

    /// <summary>
    /// Create a new reader to read from the buffer
    /// </summary>
    public MultiMemoryIStream.MultiMemoryReader CreateReader() => new(data);
    /// <summary>
    /// Create a new writer that writes to the buffer.
    /// </summary>
    public MultiMemoryIStream.MultiMemoryWriter CreateWriter() => new(data);

    /// <summary>
    /// Access the buffer data as a ReadOnlySequence
    /// </summary>
    /// <returns></returns>
    public ReadOnlySequence<byte> AsSequence() => data.AsSequence();
}

/// <summary>
/// This is an implementation class for the MultimemoryBuffer
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public closed class MultiMemoryIStream : ISeekableStream, IStreamLength, IDisposable
{
    private LinkedListPosition position;
    private readonly MultiBufferStreamList data;
    private CountedSourceTicket ticket;
    internal MultiMemoryIStream(MultiBufferStreamList data)
    {
        this.data = data;
        position = data.StartPosition;
        ticket = data.CreateSourceTicket();
    }

    /// <inheritdoc/>
    public void Seek(long value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        data.EnsureHasLocation(value);
        position = data.AsSequence().GetPosition(value);
    }

    /// <inheritdoc/>
    public void Dispose() => ticket.TryRelease();

    /// <inheritdoc/>
    public long Length => data.Length;


    /// <summary>
    /// This reader reads from a MultiBuffer
    /// </summary>
    public class MultiMemoryReader : MultiMemoryIStream, ISyncReader, IAsyncReader
    {

        internal MultiMemoryReader(MultiBufferStreamList data) : base(data)
        {
        }

        /// <inheritdoc/>
        public int Read(Span<byte> buffer)
        {
            (var ret, position) = data.Read(position, buffer);
            return ret;
        }
        /// <inheritdoc/>
        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
        {
            return ValueTask.FromResult(Read(buffer.Span));
        }
    }

    /// <summary>
    /// This writer writes to a multibuffer
    /// </summary>
    public class MultiMemoryWriter : MultiMemoryIStream, ISyncWriter, IAsyncWriter
    {
        internal MultiMemoryWriter(MultiBufferStreamList data) : base(data)
        {
        }
        /// <inheritdoc/>
        public void Write(ReadOnlySpan<byte> buffer)
        {
            position = data.Write(position, buffer);
        }
        /// <inheritdoc/>
        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellation = default)
        {
            Write(buffer.Span);
            return ValueTask.CompletedTask;
        }

    }
}