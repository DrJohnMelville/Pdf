using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using System.Buffers;
using System.ComponentModel;

namespace Melville.Parsing.LinkedLists;

/// <summary>
/// This buffer can be written to and then read by multiple independent readers.
/// It maintains its state as a linked list of buffers.  It does not reallocate to
/// grow.
/// </summary>
public readonly struct MultiMemoryBuffer : IDisposable, ISequenceSource
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
    public LinkedListIStream.LinkedListReader CreateReader() => new(data);
    /// <summary>
    /// Create a new writer that writes to the buffer.
    /// </summary>
    public LinkedListIStream.LinkedListWriter CreateWriter() => new(data);

    /// <summary>
    /// Access the buffer data as a ReadOnlySequence
    /// </summary>
    /// <returns></returns>
    public ReadOnlySequence<byte> AsSequence() => data.AsSequence();
}
