using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_1ParseSimpleWholeFile
{
    private async Task<string> Write(PdfLowLevelDocument doc)
    {
        var target = new MultiBufferStream();
        var writer = new LowLevelDocumentWriter(PipeWriter.Create(target), doc);
        await writer.WriteAsync();
        return target.CreateReader().ReadToArray().ExtendedAsciiString();
    }
    private async Task<string> OutputTwoItemDocument(byte majorVersion = 1, byte minorVersion = 7)
    {
        var builder = new LowLevelDocumentCreator();
        builder.SetVersion(majorVersion, minorVersion);
        builder.AddRootElement(
            new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Catalog).AsDictionary());
        builder.AsIndirectReference(PdfBoolean.True); // includes a dead object to be skipped
        builder.Add(new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Page).AsDictionary());
        return await Write(builder.CreateDocument());
    }

    private async Task RoundTripPdfAsync(string pdf)
    {
        var doc = await pdf.ParseDocumentAsync();
        var newPdf = await Write(doc);
        Assert.Equal(pdf, newPdf);
            
    }

    [Fact]
    public async Task GenerateDocumentWithDelayedIndirect()
    {
        var builder = new LowLevelDocumentCreator();
        var pointer = builder.AsIndirectReference();
        builder.AddRootElement(new DictionaryBuilder().WithItem(KnownNames.Width, pointer).AsDictionary());
        builder.AssignValueToReference(pointer, 10);
        builder.Add(pointer);
        var doc = await Write(builder.CreateDocument());
        var doc2 = await doc.ParseDocumentAsync();
        var rootDic = (PdfDictionary)await doc2.TrailerDictionary[KnownNames.Root];
        Assert.Equal(10, ((PdfNumber)await rootDic[KnownNames.Width]).IntValue);
    }

    [Fact]
    public async Task DocumentWithStream()
    {
        var builder = new LowLevelDocumentCreator();
        builder.AddRootElement(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Image).AsStream("Stream data"));
        var doc = builder.CreateDocument();
        var serialized = await Write(doc);
        Assert.Contains("Stream data", serialized);
        var doc2 = await serialized.ParseDocumentAsync();
        var stream = (PdfStream) (await doc2.TrailerDictionary[KnownNames.Root]);
        var value = await new StreamReader(
            await stream.StreamContentAsync(StreamFormat.DiskRepresentation)).ReadToEndAsync();
        Assert.Equal("Stream data", value);
            
    }

    [Fact]
    public async Task RoundTripSimpleDocument()
    {
        await RoundTripPdfAsync(await OutputTwoItemDocument(1, 3));
    }

    [Fact]
    public async Task ParseSimpleDocument()
    {
        var doc = await (await OutputTwoItemDocument()).ParseDocumentAsync();
        Assert.Equal(1, doc.MajorVersion);
        Assert.Equal(7, doc.MinorVersion);
        Assert.Equal(2, doc.Objects.Count);
        Assert.Equal(4, ((PdfNumber)(await doc.TrailerDictionary[KnownNames.Size])).IntValue);
        var dict = (PdfDictionary) (await doc.TrailerDictionary[KnownNames.Root]);
        Assert.Equal(KnownNames.Catalog, await dict[KnownNames.Type]);
    }
}       