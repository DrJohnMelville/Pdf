using Melville.JBig2.FileOrganization;
using Melville.JBig2.SegmentParsers;
using Melville.JBig2.Segments;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.JBig2.FileOrganization
{
    internal abstract class SegmentHeaderReader
    {
        protected ParsingBuffer Pipe { get; }
        public uint Pages { get; }
        private IReadOnlyDictionary<uint, Segment> priorSegments;
        protected SegmentHeaderReader(
            ParsingBuffer pipe, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments)
        {
            Pages = pages;
            this.priorSegments = priorSegments;
            Pipe = pipe;
        }

        public async ValueTask<SegmentReader> NextSegmentReaderAsync() =>
            new(Pipe, await NextSegmentHeaderAsync().CA(), priorSegments);
        protected abstract ValueTask<SegmentHeader> NextSegmentHeaderAsync();
    }
}

internal class SequentialSegmentHeaderReader : SegmentHeaderReader
{
    public SequentialSegmentHeaderReader(ParsingBuffer pipe, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments) :
        base(pipe, pages, priorSegments)
    {
    }

    protected override ValueTask<SegmentHeader> NextSegmentHeaderAsync() =>
        SegmentHeaderParser2.ParseAsync(Pipe);
}

internal class RandomAccessSegmentHeaderReader : SegmentHeaderReader
{
    private Queue<SegmentHeader> headers;

    public RandomAccessSegmentHeaderReader(ParsingBuffer pipe, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments, Queue<SegmentHeader> headers) : base(pipe, pages, priorSegments)
    {
        this.headers = headers;
    }

    protected override ValueTask<SegmentHeader> NextSegmentHeaderAsync() => new(headers.Dequeue());
}