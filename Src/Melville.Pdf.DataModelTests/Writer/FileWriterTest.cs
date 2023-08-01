using System;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.DataModelTests.Writer;

public class FileWriterTest
{
    private async Task<string> WriteAsync(PdfLowLevelDocument doc)
    {
        var target = new TestWriter();
        var writer = new LowLevelDocumentWriter(target.Writer, doc);
        await writer.WriteAsync();
        return target.Result();
    }

    private async Task<string> OutputSimpleDocumentAsync(byte majorVersion = 1, byte minorVersion = 7)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new ValueDictionaryBuilder().WithItem(KnownNames.TypeTName, KnownNames.CatalogTName).AsDictionary());
        return await WriteAsync(builder.CreateDocument(majorVersion, minorVersion));
    }
    private async Task<string> OutputTwoItemDocumentAsync(byte majorVersion = 1, byte minorVersion = 7)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new ValueDictionaryBuilder().WithItem(KnownNames.TypeTName, KnownNames.CatalogTName).AsDictionary());
        builder.Add(default); // includes a dead object to be skipped
        builder.Add(new ValueDictionaryBuilder().WithItem(KnownNames.TypeTName, KnownNames.PageTName).AsDictionary());
        return await WriteAsync(builder.CreateDocument(majorVersion, minorVersion));
    }
    private async Task<string> OutputTwoItemRefStreamAsync(byte majorVersion = 1, byte minorVersion = 7)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new ValueDictionaryBuilder().WithItem(KnownNames.TypeTName, KnownNames.CatalogTName).AsDictionary());
        builder.Add(PdfDirectValue.CreateNull()); // includes a dead object to be skipped
        builder.Add(new ValueDictionaryBuilder().WithItem(KnownNames.TypeTName, KnownNames.PageTName).AsDictionary());
        PdfLowLevelDocument doc = builder.CreateDocument(majorVersion, minorVersion);
        var target = new TestWriter();
        var writer = new XrefStreamLowLevelDocumentWriter(target.Writer, doc);
        await writer.WriteAsync();
        return target.Result();
    }

    [Theory]
    [InlineData(1,7)]
    [InlineData(2,7)]
    [InlineData(1,6)]
    public async Task WriteFileHeaderTestAsync(byte majorVersion, byte minorVersion)
    {
        string output = await OutputSimpleDocumentAsync(majorVersion, minorVersion);
        Assert.StartsWith(
            $"%PDF-{majorVersion}.{minorVersion}\r\n%ÿÿÿÿ Created with Melville.Pdf", output);
    }

    [Theory]
    [InlineData(10,7)]
    [InlineData(1,60)]
    public Task ThrowWhenWritingInvalidVersionNumberAsync(byte majorVersion, byte minorVersion)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new ValueDictionaryBuilder().WithItem(KnownNames.TypeTName, KnownNames.CatalogTName).AsDictionary());
        return Assert.ThrowsAsync<ArgumentException>(
            ()=> OutputSimpleDocumentAsync(majorVersion, minorVersion));
            
    }

    [Theory]
    [InlineData("Melville.Pdf\n1 0 obj <</Type/Catalog>> endobj")]
    [InlineData("endobj\nxref\n0 2\n0000000000 00000 f\r\n0000000042 00000 n\r\n")]
    [InlineData("n\r\ntrailer\n<</Root 1 0 R/Size 2>>\nstartxref\n75\n%%EOF")]
    public async Task SimpleDocumentContentsAsync(string expected) => 
        Assert.Contains(expected, await OutputSimpleDocumentAsync());

    [Theory]
    [InlineData("Melville.Pdf\n1 0 obj <</Type/Catalog>> endobj\n3 0 obj <</Type/Page>> endobj")]
    [InlineData("endobj\nxref\n0 4\n0000000002 00000 f\r\n0000000042 00000 n\r\n0000000000 00000 f\r\n0000000075 00000 n\r\n")]
    [InlineData("n\r\ntrailer\n<</Root 1 0 R/Size 4>>\nstartxref\n105\n%%EOF")]
    public async Task TwoItemDocumentContentsAsync(string expected) => 
        Assert.Contains(expected, await OutputTwoItemDocumentAsync());

    [Theory]
    [InlineData("Melville.Pdf\n1 0 obj <</Type/Catalog>> endobj\n3 0 obj <</Type/Page>> endobj")]
    [InlineData("endobj\n4 0 obj <</Root 1 0 R/Type/XRef/W[1 1 0]/Size 5/Filter/FlateDecode/DecodeParms<</Predictor 12/Columns 2>>/Length 23>> stream\r\n")]
    [InlineData("stream\r\nxÚ")]
    public async Task RefStreamContentsAsync(string expected) => 
        Assert.Contains(expected, await OutputTwoItemRefStreamAsync());
 
    [Theory]
    [InlineData(1, 1, false)]
    [InlineData(1, 4, false)]
    [InlineData(1, 5, true)]
    [InlineData(1, 6, true)]
    [InlineData(2, 0, true)]
    public async Task OnlyWriteRefStreamIfVersionAllowsAsync(byte major, byte minor, bool succeed)
    {
        try
        {
            await OutputTwoItemRefStreamAsync(major, minor);
            Assert.True(succeed);
        }
        catch (PdfParseException)
        {
            Assert.False(succeed);
        }
    }
}