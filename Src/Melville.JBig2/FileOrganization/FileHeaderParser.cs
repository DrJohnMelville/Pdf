using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.JBig2.SegmentParsers;
using Melville.JBig2.Segments;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.FileOrganization;

internal static class FileHeaderParser
{
    public static ValueTask<SegmentHeaderReader> ReadFileHeaderAsync(
        Stream input, IReadOnlyDictionary<uint, Segment> priorSegments) => 
        ReadFileHeaderAsync((PipeReader)PipeReader.Create(input), priorSegments);

    public static async ValueTask<SegmentHeaderReader> ReadFileHeaderAsync(
        PipeReader input, IReadOnlyDictionary<uint, Segment> priorSegments)
    {
        var result = await input.ReadAtLeastAsync(13).CA();
        return await ReadFileHeaderAsync(result.Buffer, input, priorSegments).CA();
    }

    private static ValueTask<SegmentHeaderReader> ReadFileHeaderAsync(
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
            CreateRandomAccessReaderAsync(input, pages, priorSegments);
    }

    private static async ValueTask<SegmentHeaderReader> CreateRandomAccessReaderAsync(
        PipeReader input, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments) => 
        new RandomAccessSegmentHeaderReader(input, pages, priorSegments,
            await ReadSegmentHeadersAsync(input).CA());

    private static async Task<Queue<SegmentHeader>> ReadSegmentHeadersAsync(PipeReader input)
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

internal abstract class SegmentHeaderReader
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

    public async ValueTask<SegmentReader> NextSegmentReaderAsync() =>
        new(Pipe, await NextSegmentHeaderAsync().CA(), priorSegments);
    protected abstract ValueTask<SegmentHeader> NextSegmentHeaderAsync();
}

internal class SequentialSegmentHeaderReader : SegmentHeaderReader
{
    public SequentialSegmentHeaderReader(PipeReader pipe, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments) : base(pipe, pages, priorSegments)
    {
    }

    protected override ValueTask<SegmentHeader> NextSegmentHeaderAsync() => SegmentHeaderParser.ParseAsync(Pipe);
}

internal class RandomAccessSegmentHeaderReader : SegmentHeaderReader
{
    private Queue<SegmentHeader> headers;

    public RandomAccessSegmentHeaderReader(PipeReader pipe, uint pages, IReadOnlyDictionary<uint, Segment> priorSegments, Queue<SegmentHeader> headers) : base(pipe, pages, priorSegments)
    {
        this.headers = headers;
    }

    protected override ValueTask<SegmentHeader> NextSegmentHeaderAsync() => new(headers.Dequeue());
}