using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;

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
        return ReadFileHeader(result.Buffer, input);
    }

    private static SegmentReader ReadFileHeader(ReadOnlySequence<byte> inputSequence, PipeReader input)
    {
        var reader = new SequenceReader<byte>(inputSequence);
        reader.Advance(8);
        var flags = (FileFlags)reader.ReadBigEndianUint8();
        uint pages = flags.HasFlag(FileFlags.UnknownPageCount) ? 0 : reader.ReadBigEndianUint32();
        return flags.HasFlag(FileFlags.SequentialOrganization)
            ? new SequentialSegmentReader(pages)
            : new RandomAccessSegmentReader(pages);
    }
}

public abstract class SegmentReader
{
    public uint Pages { get; }
    protected SegmentReader(uint pages)
    {
        Pages = pages;
    }

    public abstract ValueTask<SegmentHeader> NextSegmentHeader();
}

public class SequentialSegmentReader : SegmentReader
{
    public SequentialSegmentReader(uint pages) : base(pages)
    {
    }

    public override ValueTask<SegmentHeader> NextSegmentHeader()
    {
        throw new NotImplementedException();
    }
}

public class RandomAccessSegmentReader : SegmentReader
{
    public RandomAccessSegmentReader(uint pages) : base(pages)
    {
    }

    public override ValueTask<SegmentHeader> NextSegmentHeader()
    {
        throw new NotImplementedException();
    }
}