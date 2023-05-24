using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.Segments;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class FileParserTest
{
    private const string magicNumber = "97 4A 42 32 0D 0A 1A 0A";
    private const string endOfPageHead = "00000005 31 00 01 00000000";
    private const string endOfStripeHead = "00000002 32 00 01 00000004";
    private const string endofStripeData = "000000FF";
    private const string endOfFileHead = "00000006 33 00 01 00000000";

    private async ValueTask<SegmentHeaderReader> ParseFile(params string[] data)
    {
        return await FileHeaderParser.ReadFileHeaderAsync(new MemoryStream(BitStreamCreator.BitsFromHex(data)),
            new Dictionary<uint, Segment>());
    } 

    [Theory]
    [InlineData("01 00000003", 3u)]
    [InlineData("03", 0u)]
    public async Task ParseSequentialFileHeader(string typeAndPages, uint pages)
    {
        var reader = await ParseFile(magicNumber, typeAndPages,
            endOfStripeHead, endofStripeData, endOfPageHead, endOfFileHead);
        await VerifySimpleFile<SequentialSegmentHeaderReader>(pages, reader);
    }
    [Theory]
    [InlineData("00 00000003", 3u)]
    [InlineData("02", 0u)]
    public async Task ParseRandomAccessFileHeader(string typeAndPages, uint pages)
    {
        var reader = await ParseFile(magicNumber, typeAndPages,
            endOfStripeHead, endOfPageHead, endOfFileHead, endofStripeData);
        await VerifySimpleFile<RandomAccessSegmentHeaderReader>(pages, reader);
    }

    private static async ValueTask VerifySimpleFile<T>(uint pages, SegmentHeaderReader headerReader)
    {
        Assert.IsType<T>(headerReader);
        Assert.Equal(pages, headerReader.Pages);
        await VerifySegment(headerReader, SegmentType.EndOfStripe);
        await VerifySegment(headerReader, SegmentType.EndOfPage);
        await VerifySegment(headerReader, SegmentType.EndOfFile);
    }

    private static async Task VerifySegment(SegmentHeaderReader headerReader, SegmentType type)
    {
        var reader = await headerReader.NextSegmentReaderAsync();
        Assert.Equal(type, reader.Header.SegmentType);
        await reader.SkipOverAsync();
    }
}