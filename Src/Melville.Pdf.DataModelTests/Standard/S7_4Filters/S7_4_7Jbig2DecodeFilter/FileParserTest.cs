using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class FileParserTest
{
    private const string magicNumber = "97 4A 42 32 0D 0A 1A 0A";
    private const string endOfPageHead = "00000005 31 00 01 00000000";
    private const string endOfStripeHead = "00000002 32 00 01 00000004";
    private const string endofStripeData = "000000FF";
    private const string endOfFileHead = "00000006 33 00 01 00000000";

    private async ValueTask<SegmentReader> ParseFile(params string[] data)
    {
        return await JbigFileParser.ReadFileHeader(new MemoryStream(BitStreamCreator.BitsFromHex(data)));
    } 

    [Theory]
    [InlineData("01 00000003", 3u)]
    [InlineData("03", 0u)]
    public async Task ParseSequentialFileHeader(string typeAndPages, uint pages)
    {
        var reader = await ParseFile(magicNumber, typeAndPages,
            endOfStripeHead, endofStripeData, endOfPageHead, endOfFileHead);
        await VerifySimpleFile<SequentialSegmentReader>(pages, reader);
    }
    [Theory]
    [InlineData("00 00000003", 3u)]
    [InlineData("02", 0u)]
    public async Task ParseRandomAccessFileHeader(string typeAndPages, uint pages)
    {
        var reader = await ParseFile(magicNumber, typeAndPages,
            endOfStripeHead, endOfPageHead, endOfFileHead, endofStripeData);
        await VerifySimpleFile<RandomAccessSegmentReader>(pages, reader);
    }

    private static async ValueTask VerifySimpleFile<T>(uint pages, SegmentReader reader)
    {
        Assert.IsType<T>(reader);
        Assert.Equal(pages, reader.Pages);
        await VerifySegment(reader, 2, SegmentType.EndOfStripe);
        await VerifySegment(reader, uint.MaxValue, SegmentType.EndOfPage);
        await VerifySegment(reader, uint.MaxValue, SegmentType.EndOfFile);
    }

    private static async Task VerifySegment(SegmentReader reader, uint number, SegmentType type)
    {
        var header = await reader.NextSegment();
        Assert.Equal(number, header.Number);
        Assert.Equal(type, header.Type);
    }
}