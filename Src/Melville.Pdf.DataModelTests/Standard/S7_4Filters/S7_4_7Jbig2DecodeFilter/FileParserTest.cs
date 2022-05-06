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
    public async ValueTask<byte[]> ParseHexString(params string[] sources)
    {
        var data = string.Join("", sources.Prepend("<").Append(">"));
        return ((PdfString)await data.ParseObjectAsync()).Bytes;
    }

    private const string magicNumber = "97 4A 42 32 0D 0A 1A 0A";
    private const string endOfPageHead = "00000004 31 00 01 00000000";
    private const string endOfStripeHead = "00000004 32 00 01 00000004";
    private const string endofStripeData = "000000FF";
    private const string endOfFileHead = "00000006 33 00 01 00000000";

    private async ValueTask<SegmentReader> ParseFile(params string[] data)
    {
        return await JbigFileParser.ReadFileHeader(new MemoryStream(
            await ParseHexString(data)));
    } 

    [Theory]
    [InlineData("01 00000003", 3u)]
    [InlineData("03", 0u)]
    public async Task ParseSequentialFileHeader(string typeAndPages, uint pages)
    {
        var reader = await ParseFile(magicNumber, typeAndPages,
            endOfStripeHead, endofStripeData, endOfPageHead, endOfFileHead);
        VerifySimpleFile<SequentialSegmentReader>(pages, reader);
    }
    [Theory]
    [InlineData("00 00000003", 3u)]
    [InlineData("02", 0u)]
    public async Task ParseRandomAccessFileHeader(string typeAndPages, uint pages)
    {
        var reader = await ParseFile(magicNumber, typeAndPages,
            endOfStripeHead, endOfPageHead, endOfFileHead, endofStripeData);
        VerifySimpleFile<RandomAccessSegmentReader>(pages, reader);
    }

    private static void VerifySimpleFile<T>(uint pages, SegmentReader reader)
    {
        Assert.IsType<T>(reader);
        Assert.Equal(pages, reader.Pages);
    }
}