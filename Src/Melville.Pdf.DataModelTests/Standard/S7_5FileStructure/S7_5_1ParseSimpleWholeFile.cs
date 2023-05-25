using System.IO;
using System.IO.Pipelines;
using System.Reflection;
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
    private async Task<string> WriteAsync(PdfLowLevelDocument doc)
    {
        var target = new MultiBufferStream();
        var writer = new LowLevelDocumentWriter(PipeWriter.Create(target), doc);
        await writer.WriteAsync();
        return target.CreateReader().ReadToArray().ExtendedAsciiString();
    }
    private async Task<string> OutputTwoItemDocumentAsync(byte majorVersion = 1, byte minorVersion = 7)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Catalog).AsDictionary());
        builder.AsIndirectReference(PdfBoolean.True); // includes a dead object to be skipped
        builder.Add(new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Page).AsDictionary());
        return await WriteAsync(builder.CreateDocument(majorVersion, minorVersion));
    }

    private async Task RoundTripPdfAsync(string pdf)
    {
        var doc = await pdf.ParseDocumentAsync();
        var newPdf = await WriteAsync(doc);
        Assert.Equal(pdf, newPdf);
            
    }

    [Fact]
    public async Task GenerateDocumentWithDelayedIndirectAsync()
    {
        var builder = new LowLevelDocumentBuilder();
        var pointer = builder.CreatePromiseObject();
        builder.AddRootElement(new DictionaryBuilder().WithItem(KnownNames.Width, pointer).AsDictionary());
        pointer.SetValue(10);
        builder.Add(pointer);
        var doc = await WriteAsync(builder.CreateDocument());
        var doc2 = await doc.ParseDocumentAsync();
        var rootDic = (PdfDictionary)await doc2.TrailerDictionary[KnownNames.Root];
        Assert.Equal(10, ((PdfNumber)await rootDic[KnownNames.Width]).IntValue);
    }

    [Fact]
    public async Task DocumentWithStreamAsync()
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Image).AsStream("Stream data"));
        var doc = builder.CreateDocument();
        var serialized = await WriteAsync(doc);
        Assert.Contains("Stream data", serialized);
        var doc2 = await serialized.ParseDocumentAsync();
        var stream = (PdfStream) (await doc2.TrailerDictionary[KnownNames.Root]);
        var value = await new StreamReader(
            await stream.StreamContentAsync(StreamFormat.DiskRepresentation)).ReadToEndAsync();
        Assert.Equal("Stream data", value);
            
    }

    [Fact]
    public async Task RoundTripSimpleDocumentAsync()
    {
        await RoundTripPdfAsync(await OutputTwoItemDocumentAsync(1, 3));
    }

    [Fact]
    public async Task ParseSimpleDocumentAsync()
    {
        var doc = await (await OutputTwoItemDocumentAsync()).ParseDocumentAsync();
        Assert.Equal(1, doc.MajorVersion);
        Assert.Equal(7, doc.MinorVersion);
        Assert.Equal(2, doc.Objects.Count);
        Assert.Equal(4, ((PdfNumber)(await doc.TrailerDictionary[KnownNames.Size])).IntValue);
        var dict = (PdfDictionary) (await doc.TrailerDictionary[KnownNames.Root]);
        Assert.Equal(KnownNames.Catalog, await dict[KnownNames.Type]);
    }
}       