using System.IO.Pipelines;
using Melville.INPC;

namespace Melville.Parsing.Writers;

/// <summary>
/// This is a pipewriter that keeps track of how much it has written
/// </summary>
public partial class CountingPipeWriter: PipeWriter, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Index of the last committed byte in the writer
    /// </summary>
    public long BytesWritten { get; private set; }
    [DelegateTo] private readonly PipeWriter innerWriter;

    /// <summary>
    /// Create a new CountingPipeWriter
    /// </summary>
    /// <param name="innerWriter">The inner pipe writer to write to.</param>
    /// <param name="startPosition">The index of the first byte to be written</param>
    public CountingPipeWriter(PipeWriter innerWriter, long startPosition = 0)
    {
        this.innerWriter = innerWriter;
        BytesWritten = startPosition;
    }

    /// <inheritdoc />
    public override void Advance(int bytes)
    { 
        BytesWritten += bytes;
        innerWriter.Advance(bytes);
    }

    /// <inheritdoc />
    [Obsolete("OnReaderCompleted has been deprecated and may not be invoked on all implementations of PipeWriter.")]
    public override void OnReaderCompleted(Action<Exception?, object?> callback, object? state) => 
        innerWriter.OnReaderCompleted(callback, state);

    /// <inheritdoc />
    public void Dispose() => Complete(null);

    /// <inheritdoc />
    public ValueTask DisposeAsync() => CompleteAsync(null);
}