using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class ReadJpegStream: DefaultBaseStream, IImageSizeStream
{
    private readonly PipeReader source;
    private readonly JpegFrameData context;

    public ReadJpegStream(PipeReader source, JpegFrameData context) : 
        base(true, false, false)
    {
        this.source = source;
        this.context = context;
    }

    [DelegateTo] private IImageSizeStream SizeInfo => context;
    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        return new (0);
    }
}