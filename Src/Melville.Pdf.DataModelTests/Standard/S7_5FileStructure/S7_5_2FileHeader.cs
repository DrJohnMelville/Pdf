﻿using System.Reflection.Metadata;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public class S7_5_2FileHeader
    {
        [Theory]
        [InlineData( 1, 0)]
        [InlineData( 1, 1)]
        [InlineData( 1, 2)]
        [InlineData( 1, 3)]
        [InlineData( 1, 4)]
        [InlineData( 1, 5)]
        [InlineData( 1, 6)]
        [InlineData( 1, 7)]
        public async Task RecognizeFileVersion(int major, int minor)
        {
            var doc = await (await MinimalPdfGenerator.MinimalPdf(major,minor).AsStringAsync()).ParseDocumentAsync();
            Assert.Equal(major, doc.MajorVersion);
            Assert.Equal(minor, doc.MinorVersion);
            
        }
    }
}