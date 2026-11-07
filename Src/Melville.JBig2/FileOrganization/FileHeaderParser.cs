using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.JBig2.SegmentParsers;
using Melville.JBig2.Segments;

using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;

namespace Melville.JBig2.FileOrganization;

internal static class FileHeaderParser
{
    public static async ValueTask<SegmentHeaderReader> ReadFileHeaderAsync(
        ParsingBuffer input, IReadOnlyDictionary<uint, Segment> priorSegments)
    {
        await input.AdvanceAsync(8).CA(); // jump over the magic number
        var flags = new FileFlags(await input.GetUInt8Async().CA());
        var pages = flags.UnknownPageCount ? 0 : await input.GetUInt32BigEndianAsync().CA();

        return flags.SequentialFileOrganization
            ? new SequentialSegmentHeaderReader(input, pages, priorSegments): 
            await CreateRandomAccessReaderAsync(input, pages, priorSegments);
    }

    private static async ValueTask<SegmentHeaderReader> CreateRandomAccessReaderAsync(
        ParsingBuffer input, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments) => 
        new RandomAccessSegmentHeaderReader(input, pages, priorSegments,
            await ReadSegmentHeadersAsync(input).CA());

    private static async Task<Queue<SegmentHeader>> ReadSegmentHeadersAsync(ParsingBuffer input)
    {
        var ret = new Queue<SegmentHeader>();
        while(true)
        {
            var item = await SegmentHeaderParser2.ParseAsync(input).CA();
            ret.Enqueue(item);
            if (item.SegmentType == SegmentType.EndOfFile) return ret;
        } 
    }
}

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