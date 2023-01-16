using System;
using System.IO.Pipelines;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Writers;

internal partial class CountingPipeWriter: PipeWriter
{
    public long BytesWritten { get; private set; }
    [DelegateTo] private readonly PipeWriter innerWriter;

    public CountingPipeWriter(PipeWriter innerWriter, long startPosition = 0)
    {
        this.innerWriter = innerWriter;
        BytesWritten = startPosition;
    }

    public override void Advance(int bytes)
    { 
        BytesWritten += bytes;
        innerWriter.Advance(bytes);
    }

    [Obsolete]
    public override void OnReaderCompleted(System.Action<System.Exception?, object?> callback, object? state) => this.innerWriter.OnReaderCompleted(callback, state);

}