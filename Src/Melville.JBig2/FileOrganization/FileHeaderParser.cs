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
            var item = await SegmentHeaderParser.ParseAsync(input).CA();
            ret.Enqueue(item);
            if (item.SegmentType == SegmentType.EndOfFile) return ret;
        } 
    }
}

