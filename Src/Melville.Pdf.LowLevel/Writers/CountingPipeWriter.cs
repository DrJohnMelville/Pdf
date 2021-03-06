using System.IO.Pipelines;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Writers;

public partial class CountingPipeWriter: PipeWriter
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
}