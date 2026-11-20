using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.ReferenceDocuments.LowLevel;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_2FileHeader
{
    [Test]
    [Arguments( 1, 0)]
    [Arguments( 1, 1)]
    [Arguments( 1, 2)]
    [Arguments( 1, 3)]
    [Arguments( 1, 4)]
    [Arguments( 1, 5)]
    [Arguments( 1, 6)]
    [Arguments( 1, 7)]
    public async Task RecognizeFileVersionAsync(int major, int minor)
    {
        using var doc = await (await MinimalPdfParser.MinimalPdf((byte)major, (byte)minor)
            .AsStringAsync()).ParseDocumentAsync();
        Assert.Equal(major, doc.MajorVersion);
        Assert.Equal(minor, doc.MinorVersion);
            
    }

    [Test]
    public async Task ParseWithLeadingWhiteSpaceAsync()
    {
        var docString = "   \r\n" + (await MinimalPdfParser.MinimalPdf()
            .AsStringAsync());
        using var doc = await docString.ParseDocumentAsync();
        Assert.Equal(1, doc.MajorVersion);
        Assert.Equal(7, doc.MinorVersion);
    }
}