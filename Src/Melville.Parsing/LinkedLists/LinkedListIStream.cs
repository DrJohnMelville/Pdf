using Melville.Parsing.MultiplexSources;
using Melville.StreamInterfaces;
using System.ComponentModel;

namespace Melville.Parsing.LinkedLists;


/// <summary>
/// This is an implementation class for the MultimemoryBuffer
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public closed class LinkedListIStream : ISeekableStream, IStreamLength, IDisposable,IHasSourceTicket
{
    private LinkedListPosition position;
    private readonly LinkedList data;
    CountedSourceTicket IHasSourceTicket.SourceTicket { get; set; }
    internal LinkedListIStream(LinkedList data)
    {
        this.data = data;
        position = data.StartPosition;
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
    public void Dispose() => ((IHasSourceTicket)this).SourceTicket.Release(this);

    /// <inheritdoc/>
    public long Length => data.Length;


    /// <summary>
    /// This reader reads from a MultiBuffer
    /// </summary>
    public class LinkedListReader : LinkedListIStream, ISyncReader, IAsyncReader
    {

        internal LinkedListReader(LinkedList data) : base(data)
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
    public class LinkedListWriter : LinkedListIStream, ISyncWriter, IAsyncWriter
    {
        internal LinkedListWriter(LinkedList data) : base(data)
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