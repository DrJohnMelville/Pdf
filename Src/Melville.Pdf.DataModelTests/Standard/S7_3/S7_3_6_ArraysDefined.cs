﻿using System.Collections;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3
{
    public class S7_3_6_ArraysDefined
    {
        [Theory]
        [InlineData("[] /fd", 0)]
        [InlineData("[123.5] sd", 1)]
        [InlineData("[123.5 (this is a string () inside)]/wdg", 2)]
        [InlineData("[true false null] /", 3)]
        [InlineData("[[true false] null] /", 2)]
        [InlineData("[/WIDTH /HGH /X1 /HEIGHT] /aaz", 4)]
        public async Task ParseArray(string src, int length)
        {
            var obj = (PdfArray) await src.ParseObjectAsync();
            Assert.Equal(length, obj.RawItems.Count);
        }

        [Fact]
        public async Task ArrayIsAReadOnlyListOfTheDirectObjectsInTheArray()
        {
            var sut = (PdfArray)await "[ 1 0 R false null 1 0 obj true endobj ]  ".ParseObjectAsync();
            Assert.Equal(4, sut.Count);
            Assert.Equal(PdfBoolean.True, sut[0]);
            Assert.Equal(PdfBoolean.False, sut[1]);
            Assert.Equal(PdfEmptyConstants.Null, sut[2]);
            Assert.Equal(PdfBoolean.True, sut[3]);

            Assert.Equal(new PdfObject[]
                    {PdfBoolean.True, PdfBoolean.False, PdfEmptyConstants.Null, PdfBoolean.True},
                (IEnumerable)sut);
            
        }
    }
}