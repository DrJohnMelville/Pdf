using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocuments.LowLevel;
using Microsoft.Win32;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S_7_5_8CrossReferenceStreams
{
    [Fact]
    public async Task GenerateAndParseFileWithReferenceStream()
    {
        var document = MinimalPdfParser.MinimalPdf(1, 7);
        var ms = new MultiBufferStream();
        var writer = new LowLevelDocumentWriter(PipeWriter.Create(ms), document.CreateDocument());
        await writer.WriteWithReferenceStream();
        var fileAsString = ms.CreateReader().ReadToArray().ExtendedAsciiString();
        Assert.DoesNotContain(fileAsString, "trailer");
        var doc = await (fileAsString).ParseDocumentAsync();
        Assert.NotNull(doc.TrailerDictionary);
        Assert.IsType<PdfStream>(doc.TrailerDictionary);
        var root = await doc.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root);
        var pages = await root.GetAsync<PdfDictionary>(KnownNames.Pages);
        var kids = await pages.GetAsync<PdfArray>(KnownNames.Kids);
        await Assert.Single(kids);
    }

    [Fact]
    public async Task ParseSingleDeletedItem()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.Index, new PdfArray(10, 1))
            .AsStream(new byte[] { 0, 1, 2 });
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamData(target.Object, xs);
        
        target.Verify(i=>i.RegisterDeletedBlock(10, 2, 1));
        target.VerifyNoOtherCalls();
    }

    private static DictionaryBuilder XrefBase()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XRef)
            .WithItem(KnownNames.W, new PdfArray(1, 1, 1));
    }

    [Fact]
    public async Task ParseSingleIndirectItem()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.Index, new PdfArray(10, 1))
            .AsStream(new byte[] { 1, 1, 2 });
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamData(target.Object, xs);
        
        target.Verify(i=>i.RegisterIndirectBlock(10, 2, 1));
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task ParseSingleStreamItem()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.Index, new PdfArray(10, 1))
            .AsStream(new byte[] { 2, 1, 2 });
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamData(target.Object, xs);
        
        target.Verify(i=>i.RegisterObjectStreamBlock(10, 1, 2));
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task ParseTrippleIndirectItem()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.Index, new PdfArray(10, 3))
            .AsStream(new byte[] { 1, 1, 2,   1,110,21,  1,50, 0});
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamData(target.Object, xs);
        
        target.Verify(i=>i.RegisterIndirectBlock(10, 2, 1));
        target.Verify(i=>i.RegisterIndirectBlock(11, 21, 110));
        target.Verify(i=>i.RegisterIndirectBlock(12, 0, 50));
        target.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task IndirectWithMultipleSections()
    {
        var xs = XrefBase()
            .WithItem(KnownNames.Index, new PdfArray(10, 2,  255,1))
            .AsStream(new byte[] { 1, 1, 2,   1,110,21,  1,50, 0});
        
        var target = new Mock<IIndirectObjectRegistry>();

        await CrossReferenceStreamParser.ReadXrefStreamData(target.Object, xs);
        
        target.Verify(i=>i.RegisterIndirectBlock(10, 2, 1));
        target.Verify(i=>i.RegisterIndirectBlock(11, 21, 110));
        target.Verify(i=>i.RegisterIndirectBlock(255, 0, 50));
        target.VerifyNoOtherCalls();
    }

}