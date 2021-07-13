using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public class S7_5_4CrossReferenceTable
    {

        private readonly Mock<IIndirectObjectResolver> resolver = new();
        
        [Fact]
        public async Task ParseSimpleTable()
        {
            var sampleTale = @"xref
0 6
0000000003 65535 f
0000000017 00000 n
0000000081 00000 n
0000000000 00007 f
0000000331 00122 n
0000000409 00000 n

";
            await new CrossReferenceTableParser(sampleTale.AsParsingSource(resolver.Object)).Parse();
            
            resolver.Verify(i=>i.AddLocationHint(1,0,17), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(2,0,81), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(4,122,331), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(5, 0, 409), Times.Once);
        }
        [Fact]
        public async Task ParseCompoundTable()
        {
            var sampleTale = @"xref
0 4
0000000003 65535 f
0000000017 00000 n
0000000081 00000 n
0000000000 00007 f
23 2
0000000331 00122 n
0000000409 00000 n

";
            await new CrossReferenceTableParser(sampleTale.AsParsingSource(resolver.Object)).Parse();
            
            resolver.Verify(i=>i.AddLocationHint(1,0,17), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(2,0,81), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(23,122,331), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(24, 0, 409), Times.Once);
            resolver.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LodFileTableWhenLoadingFiles()
        {
            var resolver = new Mock<IIndirectObjectResolver>();
            var ps = MinimalPdfGenerator.MinimalPdf(1,5).AsParsingSource(resolver.Object);
            await new RandomAccessFileParser(ps).Parse();
            resolver.Verify(i=>i.AddLocationHint(1,0,9), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(2,0,74), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(3,0,119), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(4,0,176), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(5,0,295), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(6,0,376), Times.Once);
            resolver.Verify(i=>i.FindIndirect(1,0));
            resolver.VerifyNoOtherCalls();
        }
    }
}