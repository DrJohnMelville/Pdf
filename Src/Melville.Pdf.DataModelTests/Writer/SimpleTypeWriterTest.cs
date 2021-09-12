using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer
{
    public class SimpleTypeWriterTest
    {
        [Fact]
        public async Task WriteTokens()
        {
            Assert.Equal("true",await PdfBoolean.True.WriteToStringAsync());
            Assert.Equal("false",await PdfBoolean.False.WriteToStringAsync());
            Assert.Equal("null",await PdfTokenValues.Null.WriteToStringAsync());
            Assert.Equal("]",await PdfTokenValues.ArrayTerminator.WriteToStringAsync());
            Assert.Equal(">>",await PdfTokenValues.DictionaryTerminator.WriteToStringAsync());
            
        }
        
        [Theory]
        [InlineData("Hello", "(Hello)")]
        [InlineData("", "()")]
        [InlineData("\n", "(\\n)")]
        [InlineData("\r", "(\\r)")]
        [InlineData("\t", "(\\t)")]
        [InlineData("\b", "(\\b)")]
        [InlineData("\f", "(\\f)")]
        [InlineData("\u0000", "(\u0000)")]
        [InlineData(@"this is a \Test", @"(this is a \\Test)")]
        [InlineData(@"this is a (Test", @"(this is a \(Test)")]
        [InlineData(@"this is a )Test", @"(this is a \)Test)")]
        public async Task WriteStrings(string source, string dest)
        {
            Assert.Equal(dest, await new PdfString(source).WriteToStringAsync());
            Assert.Equal(source, (await dest.ParseObjectAsync()).ToString());
            
        }
        [Theory]
        [InlineData("Hello", "/Hello")]
        [InlineData("Hel#lo", "/Hel#23lo")]
        [InlineData("Hel lo", "/Hel#20lo")]
        public async Task WriteName(string source, string dest)
        {
            Assert.Equal(dest, await new PdfName(source).WriteToStringAsync());
        }
        [Theory]
        [InlineData(0, "0")]
        [InlineData(1234, "1234")]
        [InlineData(-1234, "-1234")]
        public async Task WriteIntegers(int source, string dest)
        {
            Assert.Equal(dest, await new PdfInteger(source).WriteToStringAsync());
        }
        [Theory]
        [InlineData(0, "0")]
        [InlineData(1234, "1234")]
        [InlineData(-1234, "-1234")]
        [InlineData(-1234.54, "-1234.54")]
        public async Task WriteDoubles(double source, string dest)
        {
            Assert.Equal(dest, await new PdfDouble(source).WriteToStringAsync());
        }

        [Fact]
        public async Task WriteIndirectObjectReference()
        {
            var reference = new PdfIndirectReference(new PdfIndirectObject(34, 555, PdfBoolean.False));
            Assert.Equal("34 555 R", await reference.WriteToStringAsync());

        }
        [Fact]
        public async Task WriteIndirectObject()
        {
            var reference = new PdfIndirectObject(34, 555, PdfBoolean.False);
            Assert.Equal("34 555 obj false endobj\n", await reference.WriteToStringAsync());
        }
        [Fact]
        public async Task WriteArray()
        {
            var array = new PdfArray(new[]
            {
                PdfBoolean.True, PdfBoolean.False, PdfTokenValues.Null
            });
            Assert.Equal("[true false null]", await array.WriteToStringAsync());
        }
        [Fact]
        public async Task WriteDictionary()
        {
            var array = new PdfDictionary(new Dictionary<PdfName, PdfObject>()
            {
                {KnownNames.Width, new PdfInteger(20)},
                {KnownNames.Height, new PdfInteger(40)},
            });
            Assert.Equal("<</Width 20/Height 40>>", await array.WriteToStringAsync());
        }

        [Fact]
        public async Task WriteStream()
        {
            var array = new PdfStream(new LiteralStreamSource("Hello"), new Dictionary<PdfName, PdfObject>()
            {
                {KnownNames.Length, new PdfInteger(5)},
            });
            Assert.Equal("<</Length 5>> stream\r\nHello\r\nendstream", await array.WriteToStringAsync());
        }
    }
}