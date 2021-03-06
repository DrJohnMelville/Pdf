using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;

public static class FileHeaderParser
{
    public static ValueTask<SegmentHeaderReader> ReadFileHeader(
        Stream input, IReadOnlyDictionary<uint, Segment> priorSegments) => 
        ReadFileHeader(PipeReader.Create(input), priorSegments);

    public static async ValueTask<SegmentHeaderReader> ReadFileHeader(
        PipeReader input, IReadOnlyDictionary<uint, Segment> priorSegments)
    {
        var result = await input.ReadAtLeastAsync(13).CA();
        return await ReadFileHeader(result.Buffer, input, priorSegments).CA();
    }

    private static ValueTask<SegmentHeaderReader> ReadFileHeader(
        ReadOnlySequence<byte> inputSequence, PipeReader input, 
        IReadOnlyDictionary<uint, Segment> priorSegments)
    {
        var reader = new SequenceReader<byte>(inputSequence);
        reader.Advance(8);
        var flags = new FileFlags(reader.ReadBigEndianUint8());
        var pages = flags.UnknownPageCount ? 0 : reader.ReadBigEndianUint32();
        input.AdvanceTo(reader.Position);
        return flags.SequentialFileOrganization
            ? new ValueTask<SegmentHeaderReader>(
                new SequentialSegmentHeaderReader(input, pages, priorSegments)): 
            CreateRandomAccessReader(input, pages, priorSegments);
    }

    private static async ValueTask<SegmentHeaderReader> CreateRandomAccessReader(
        PipeReader input, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments) => 
        new RandomAccessSegmentHeaderReader(input, pages, priorSegments,
            await ReadSegmentHeaders(input).CA());

    private static async Task<Queue<SegmentHeader>> ReadSegmentHeaders(PipeReader input)
    {
        var ret = new Queue<SegmentHeader>();
        while(true)
        {
            var item = await SegmentHeaderParser.ParseAsync(input).CA();
            ret.Enqueue(item);
            if (item.SegmentType == SegmentType.EndOfFile) return ret;
        } 
    }
}

public abstract class SegmentHeaderReader
{
    protected PipeReader Pipe { get; }
    public uint Pages { get; }
    private IReadOnlyDictionary<uint, Segment> priorSegments;
    protected SegmentHeaderReader(
        PipeReader pipe, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments)
    {
        Pages = pages;
        this.priorSegments = priorSegments;
        Pipe = pipe;
    }

    public async ValueTask<SegmentReader> NextSegmentReader() =>
        new(Pipe, await NextSegmentHeader().CA(), priorSegments);
    protected abstract ValueTask<SegmentHeader> NextSegmentHeader();
}

public class SequentialSegmentHeaderReader : SegmentHeaderReader
{
    public SequentialSegmentHeaderReader(PipeReader pipe, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments) : base(pipe, pages, priorSegments)
    {
    }

    protected override ValueTask<SegmentHeader> NextSegmentHeader() => SegmentHeaderParser.ParseAsync(Pipe);
}

public class RandomAccessSegmentHeaderReader : SegmentHeaderReader
{
    private Queue<SegmentHeader> headers;

    public RandomAccessSegmentHeaderReader(PipeReader pipe, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments, Queue<SegmentHeader> headers) : base(pipe, pages, priorSegments)
    {
        this.headers = headers;
    }

    protected override ValueTask<SegmentHeader> NextSegmentHeader() => new(headers.Dequeue());
}