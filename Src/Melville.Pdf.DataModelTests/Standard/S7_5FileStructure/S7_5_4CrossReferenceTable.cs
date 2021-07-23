﻿using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
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
            await new CrossReferenceTableParser(await sampleTale.AsParsingSource(resolver.Object).RentReader(0)).Parse();
            
            resolver.Verify(i=>i.AddLocationHint(1,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(2,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(4,122,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(5, 0, It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
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
           var firstFree =  await new CrossReferenceTableParser(await sampleTale.AsParsingSource(resolver.Object).RentReader(0)).Parse();

           Assert.Equal(3, firstFree);
           
            resolver.Verify(i=>i.AddLocationHint(1,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(2,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(23,122,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(24, 0, It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.VerifyNoOtherCalls();
        }
        [Fact]
        public async Task ParseZeroTable()
        {
            var sampleTale = @"xref
0 0
trailer
";
            var parsingReader = await sampleTale.AsParsingSource(resolver.Object).RentReader(0);
            var firstFree = await new CrossReferenceTableParser(parsingReader).Parse();
            Assert.Equal(0, firstFree);
            Assert.Equal(11, parsingReader.Position);
                        
            resolver.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LodFileTableWhenLoadingFiles()
        {
            var resolver = new Mock<IIndirectObjectResolver>();
            var ps = (await MinimalPdfGenerator.MinimalPdf(1,5).AsStringAsync()).AsParsingSource(resolver.Object);
            await RandomAccessFileParser.Parse(ps);
            resolver.Verify(i=>i.AddLocationHint(1,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(2,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(3,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(4,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(5,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.AddLocationHint(6,0,It.IsAny<Func<ValueTask<PdfObject>>>()), Times.Once);
            resolver.Verify(i=>i.FindIndirect(1,0));
            resolver.Verify(i=>i.GetObjects());
            resolver.VerifyNoOtherCalls();
        }
    }
}