using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocuments.LowLevel;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S_7_5_8CrossReferenceStreams
{
    [Fact]
    public async Task GenerateAndParseFileWithReferenceStreamAsync()
    {
        var document = MinimalPdfParser.MinimalPdf();
        var ms = new MultiBufferStream();
        var writer = new XrefStreamLowLevelDocumentWriter(PipeWriter.Create(ms), document);
        await writer.WriteAsync();
        var docAsArrat = ms.CreateReader().ReadToArray();
        var fileAsString = docAsArrat.ExtendedAsciiString();
        Assert.DoesNotContain(fileAsString, "trailer");
        var doc = await (fileAsString).ParseDocumentAsync();
        Assert.NotNull(doc.TrailerDictionary);
        Assert.IsType<PdfValueStream>(doc.TrailerDictionary);
        var root = await doc.TrailerDictionary.GetAsync<PdfValueDictionary>(KnownNames.RootTName);
        var pages = await root.GetAsync<PdfValueDictionary>(KnownNames.PagesTName);
        var kids = await pages.GetAsync<PdfValueArray>(KnownNames.KidsTName);
        await Assert.Single(kids);
    }

    [Fact]
    public async Task ParseSingleDeletedItemAsync()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.IndexTName, new PdfValueArray(10, 1))
            .AsStream(new byte[] { 0, 1, 2 });
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamDataAsync(target.Object, xs);
        
        target.Verify(i=>i.RegisterDeletedBlock(10, 2, 1));
        target.VerifyNoOtherCalls();
    }

    private static ValueDictionaryBuilder XrefBase()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XRefTName)
            .WithItem(KnownNames.WTName, new PdfValueArray(1, 1, 1));
    }

    [Fact]
    public async Task ParseSingleIndirectItemAsync()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.IndexTName, new PdfValueArray(10, 1))
            .AsStream(new byte[] { 1, 1, 2 });
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamDataAsync(target.Object, xs);
        
        target.Verify(i=>i.RegisterIndirectBlock(10, 2, 1));
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task ParseSingleStreamItemAsync()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.IndexTName, new PdfValueArray(10, 1))
            .AsStream(new byte[] { 2, 1, 2 });
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamDataAsync(target.Object, xs);
        
        target.Verify(i=>i.RegisterObjectStreamBlock(10, 1, 2));
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task ParseTrippleIndirectItemAsync()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.IndexTName, new PdfValueArray(10, 3))
            .AsStream(new byte[] { 1, 1, 2,   1,110,21,  1,50, 0});
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamDataAsync(target.Object, xs);
        
        target.Verify(i=>i.RegisterIndirectBlock(10, 2, 1));
        target.Verify(i=>i.RegisterIndirectBlock(11, 21, 110));
        target.Verify(i=>i.RegisterIndirectBlock(12, 0, 50));
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task IndirectWithMultipleSectionsAsync()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.IndexTName, new PdfValueArray(10, 2,  255,1))
            .AsStream(new byte[] { 1, 1, 2,   1,110,21,  1,50, 0});
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamDataAsync(target.Object, xs);
        
        target.Verify(i=>i.RegisterIndirectBlock(10, 2, 1));
        target.Verify(i=>i.RegisterIndirectBlock(11, 21, 110));
        target.Verify(i=>i.RegisterIndirectBlock(255, 0, 50));
        target.VerifyNoOtherCalls();
    }

}