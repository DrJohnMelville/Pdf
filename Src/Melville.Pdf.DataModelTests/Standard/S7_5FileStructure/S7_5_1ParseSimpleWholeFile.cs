using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_1ParseSimpleWholeFile
{
    private async Task<string> WriteAsync(PdfLowLevelDocument doc)
    {
        using var target = WritableBuffer.Create();
        using var pipeWriter = target.WritingPipe();
        var writer = new LowLevelDocumentWriter(pipeWriter, doc);
        await writer.WriteAsync();
        await using var reader = target.ReadFrom(0);
        return  reader.ReadToArray().ExtendedAsciiString();
    }
    private async Task<string> OutputTwoItemDocumentAsync(byte majorVersion = 1, byte minorVersion = 7)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.AddRootElement(
            new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.Catalog).AsDictionary());
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
        var pointer = builder.Add(default);
        builder.AddRootElement(new DictionaryBuilder().WithItem(KnownNames.Width, pointer).AsDictionary());
        builder.Reassign(pointer, 10);
        var doc = await WriteAsync(builder.CreateDocument());
        var doc2 = await doc.ParseDocumentAsync();
        var rootDic = await doc2.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root);
        Assert.Equal(10, (await rootDic[KnownNames.Width]).Get<int>());
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
        var stream =  await doc2.TrailerDictionary.GetAsync<PdfStream>(KnownNames.Root);
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
        Assert.Equal(3, (await doc.TrailerDictionary[KnownNames.Size]).Get<int>());
        var dict =  (await doc.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root));
        Assert.Equal(KnownNames.Catalog, await dict[KnownNames.Type]);
    }
}       