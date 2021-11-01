using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_4CrossReferenceTable
{

    private readonly Mock<IIndirectObjectResolver> resolver = new();
        
    [Fact]
    public async Task ParseSimpleTable()
    {
        var sampleTale = @"0 6
0000000003 65535 f
0000000017 00000 n
0000000081 00000 n
0000000000 00007 f
0000000331 00122 n
0000000409 00000 n

";
        await new CrossReferenceTableParser(await sampleTale.AsParsingSource(resolver.Object).RentReader(0)).Parse();
            
        resolver.Verify(i=>i.AddLocationHint(1,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(2,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(4,122,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(5, 0, It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
    }
    [Fact]
    public async Task ParseCompoundTable()
    {
        var sampleTale = @"0 4
0000000003 65535 f
0000000017 00000 n
0000000081 00000 n
0000000000 00007 f
23 2
0000000331 00122 n
0000000409 00000 n

"; 
        await new CrossReferenceTableParser(await sampleTale.AsParsingSource(resolver.Object).RentReader(0)).Parse();

        resolver.Verify(i=>i.AddLocationHint(0,65535,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(1,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(2,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(3,7,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(23,122,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(24, 0, It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task ParseZeroTable()
    {
        var sampleTale = @"0 0
trailer
";
        var parsingReader = await sampleTale.AsParsingSource(resolver.Object).RentReader(0);
        await new CrossReferenceTableParser(parsingReader).Parse();
        Assert.Equal(5, parsingReader.GlobalPosition);
                        
        resolver.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LodFileTableWhenLoadingFiles()
    {
        var resolver = new Mock<IIndirectObjectResolver>();
        var file = await MinimalPdfParser.MinimalPdf(1, 5).AsStringAsync();
        var ps = (file).AsParsingSource(resolver.Object);
        await RandomAccessFileParser.Parse(ps);
        resolver.Verify(i=>i.AddLocationHint(0,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(1,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(2,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(3,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.AddLocationHint(4,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
        resolver.Verify(i=>i.FindIndirect(4,0));
        resolver.Verify(i=>i.GetObjects());
        resolver.Verify(i=>i.FreeListHead(), Times.Once);
        resolver.VerifyNoOtherCalls();
    }
}