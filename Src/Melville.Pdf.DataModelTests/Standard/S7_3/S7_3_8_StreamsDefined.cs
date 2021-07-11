﻿using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7
{
    public class S7_3_8_StreamsDefined
    {
        [Fact]
        public async Task ParseSimpleStream()
        {
            var obj = (PdfStream)await "<</LENGTH 6>> stream\r\n123456\r\nendstream".ParseObjectAsync();
            Assert.Equal(22, obj.SourceFilePosition);
        }        
        [Fact]
        public async Task ParseSimpleStreamAfterJump()
        {
            var parser =  "          <</LENGTH 6>> stream\r\n123456\r\nendstream".AsParsingSource();
            var obj = (PdfStream)await parser.ParseObjectAsync();
            Assert.Equal(32, obj.SourceFilePosition);
            parser.Seek(5);
            var obj2 =  (PdfStream) await parser.ParseObjectAsync();
            Assert.Equal(32, obj2.SourceFilePosition);
        }        
        [Fact]
        public async Task ParseStreamWithMissingData()
        {
            var obj = (PdfStream)await "<</LENGTH 6>> stream\r\n".ParseObjectAsync();
            Assert.Equal(22, obj.SourceFilePosition);
        }        
    }
}