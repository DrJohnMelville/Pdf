using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Xml.XPath;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter;

[Flags]
public enum FileFlags : byte
{
    SequentialOrganization = 1,
    UnknownPageCount = 2
}

public static class JbigFileParser
{
    public static ValueTask<SegmentReader> ReadFileHeader(Stream input) => ReadFileHeader(PipeReader.Create(input));

    private static async ValueTask<SegmentReader> ReadFileHeader(PipeReader input)
    {
        var result = await input.ReadAtLeastAsync(13).CA();
        return await ReadFileHeader(result.Buffer, input).CA();
    }

    private static ValueTask<SegmentReader> ReadFileHeader(ReadOnlySequence<byte> inputSequence, PipeReader input)
    {
        var reader = new SequenceReader<byte>(inputSequence);
        reader.Advance(8);
        var flags = (FileFlags)reader.ReadBigEndianUint8();
        var pages = flags.HasFlag(FileFlags.UnknownPageCount) ? 0 : reader.ReadBigEndianUint32();
        input.AdvanceTo(reader.Position);
        return flags.HasFlag(FileFlags.SequentialOrganization)
            ? new ValueTask<SegmentReader>(new SequentialSegmentReader(input, pages)): 
            CreateRandomAccessReader(input, pages);
    }

    private static async ValueTask<SegmentReader> CreateRandomAccessReader(PipeReader input, uint pages) => 
        new RandomAccessSegmentReader(input, pages, await ReadSegmentHeaders(input).CA());

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

public abstract class SegmentReader
{
    protected PipeReader Pipe { get; }
    public uint Pages { get; }
    protected SegmentReader(PipeReader pipe, uint pages)
    {
        Pages = pages;
        Pipe = pipe;
    }

    public async ValueTask<Segment> NextSegment()
    {
        var header = await NextSegmentHeader().CA();
        return await header.ReadFromAsync(Pipe).CA();
    }

    protected abstract ValueTask<SegmentHeader> NextSegmentHeader();
}

public class SequentialSegmentReader : SegmentReader
{
    public SequentialSegmentReader(PipeReader pipe, uint pages) : base(pipe, pages)
    {
    }

    protected override ValueTask<SegmentHeader> NextSegmentHeader() => SegmentHeaderParser.ParseAsync(Pipe);
}

public class RandomAccessSegmentReader : SegmentReader
{
    private Queue<SegmentHeader> headers;
    public RandomAccessSegmentReader(PipeReader pipe, uint pages, Queue<SegmentHeader> headers) : base(pipe, pages)
    {
        this.headers = headers;
    }

    protected override ValueTask<SegmentHeader> NextSegmentHeader() => new(headers.Dequeue());
}