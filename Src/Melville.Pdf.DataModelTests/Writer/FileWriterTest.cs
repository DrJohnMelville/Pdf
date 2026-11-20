using System;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.DataModelTests.Writer;

public class FileWriterTest
{
    private async Task<string> WriteAsync(PdfLowLevelDocument doc)
    {
        using var target = new TestWriter();
        var writer = new LowLevelDocumentWriter(target.Writer, doc);
        await writer.WriteAsync();
        return target.Result();
    }

    private async Task<string> OutputSimpleDocumentAsync(byte majorVersion = 1, byte minorVersion = 7)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Catalog).AsDictionary());
        return await WriteAsync(builder.CreateDocument(majorVersion, minorVersion));
    }
    private async Task<string> OutputTwoItemDocumentAsync(byte majorVersion = 1, byte minorVersion = 7)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Catalog).AsDictionary());
        builder.Add(default); // includes a dead object to be skipped
        builder.Add(new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Page).AsDictionary());
        return await WriteAsync(builder.CreateDocument(majorVersion, minorVersion));
    }
    private async Task<string> OutputTwoItemRefStreamAsync(byte majorVersion = 1, byte minorVersion = 7)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Catalog).AsDictionary());
        builder.Add(PdfDirectObject.CreateNull()); // includes a dead object to be skipped
        builder.Add(new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Page).AsDictionary());
        PdfLowLevelDocument doc = builder.CreateDocument(majorVersion, minorVersion);
        using var target = new TestWriter();
        var writer = new XrefStreamLowLevelDocumentWriter(target.Writer, doc);
        await writer.WriteAsync();
        return target.Result();
    }

    [Test]
    [Arguments(1,7)]
    [Arguments(2,7)]
    [Arguments(1,6)]
    public async Task WriteFileHeaderTestAsync(byte majorVersion, byte minorVersion)
    {
        string output = await OutputSimpleDocumentAsync(majorVersion, minorVersion);
        Assert.StartsWith(
            $"%PDF-{majorVersion}.{minorVersion}\r\n%ÿÿÿÿ Created with Melville.Pdf", output);
    }

    [Test]
    [Arguments(10,7)]
    [Arguments(1,60)]
    public Task ThrowWhenWritingInvalidVersionNumberAsync(byte majorVersion, byte minorVersion)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Catalog).AsDictionary());
        return Assert.ThrowsAsync<ArgumentException>(
            ()=> OutputSimpleDocumentAsync(majorVersion, minorVersion));
            
    }

    [Test]
    [Arguments("Melville.Pdf\n1 0 obj <</Type/Catalog>> endobj")]
    [Arguments("endobj\nxref\n0 2\n0000000000 00000 f\r\n0000000042 00000 n\r\n")]
    [Arguments("n\r\ntrailer\n<</Root 1 0 R/Size 2>>\nstartxref\n75\n%%EOF")]
    public async Task SimpleDocumentContentsAsync(string expected)
    {
        Assert.Contains(expected, await OutputSimpleDocumentAsync());
    }

    [Test]
    [Arguments("Melville.Pdf\n1 0 obj <</Type/Catalog>> endobj\n3 0 obj <</Type/Page>> endobj")]
    [Arguments("endobj\nxref\n0 4\n0000000002 00000 f\r\n0000000042 00000 n\r\n0000000000 00000 f\r\n0000000075 00000 n\r\n")]
    [Arguments("n\r\ntrailer\n<</Root 1 0 R/Size 4>>\nstartxref\n105\n%%EOF")]
    public async Task TwoItemDocumentContentsAsync(string expected) => 
        Assert.Contains(expected, await OutputTwoItemDocumentAsync());

    [Test]
    [Arguments("Melville.Pdf\n1 0 obj <</Type/Catalog>> endobj\n3 0 obj <</Type/Page>> endobj")]
    [Arguments("endobj\n4 0 obj <</Root 1 0 R/Type/XRef/W[1 1 0]/Size 5/Filter/FlateDecode/DecodeParms<</Predictor 12/Columns 2>>/Length 27>> stream\r\n")]
    [Arguments("stream\r\nxÚ")]
    public async Task RefStreamContentsAsync(string expected)
    {
        string actualString = await OutputTwoItemRefStreamAsync();
        Assert.Contains(expected, actualString);
    }

    [Test]
    [Arguments(1, 1, false)]
    [Arguments(1, 4, false)]
    [Arguments(1, 5, true)]
    [Arguments(1, 6, true)]
    [Arguments(2, 0, true)]
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