using System;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer
{
    public class FileWriterTest
    {
        private async Task<string> Write(PdfLowLevelDocument doc)
        {
            var target = new TestWriter();
            var writer = new LowLevelDocumentWriter(target.Writer);
            await writer.WriteAsync(doc);
            return target.Result();
        }

        private async Task<string> OutputSimpleDocument(byte majorVersion = 1, byte minorVersion = 7)
        {
            var builder = new LowLevelDocumentCreator();
            builder.SetVersion(majorVersion, minorVersion);
            builder.AddRootElement(new PdfDictionary((KnownNames.Type, KnownNames.Catalog)));
            return await Write(builder.CreateDocument());
        }
        private async Task<string> OutputTwoItemDocument(byte majorVersion = 1, byte minorVersion = 7)
        {
            var builder = new LowLevelDocumentCreator();
            builder.SetVersion(majorVersion, minorVersion);
            builder.AddRootElement(new PdfDictionary((KnownNames.Type, KnownNames.Catalog)));
            builder.AsIndirectReference(PdfBoolean.True); // includes a dead object to be skipped
            builder.Add(new PdfDictionary((KnownNames.Type, KnownNames.Page)));
            return await Write(builder.CreateDocument());
        }
        private async Task<string> OutputTwoItemRefStream(byte majorVersion = 1, byte minorVersion = 7)
        {
            var builder = new LowLevelDocumentCreator();
            builder.SetVersion(majorVersion, minorVersion);
            builder.AddRootElement(new PdfDictionary((KnownNames.Type, KnownNames.Catalog)));
            builder.AsIndirectReference(PdfBoolean.True); // includes a dead object to be skipped
            builder.Add(new PdfDictionary((KnownNames.Type, KnownNames.Page)));
            PdfLowLevelDocument doc = builder.CreateDocument();
            var target = new TestWriter();
            var writer = new LowLevelDocumentWriter(target.Writer);
            await writer.WriteWithReferenceStream(doc);
            return target.Result();
        }

        [Theory]
        [InlineData(1,7)]
        [InlineData(2,7)]
        [InlineData(1,6)]
        public async Task WriteFileHeaderTest(byte majorVersion, byte minorVersion)
        {
            string output = await OutputSimpleDocument(majorVersion, minorVersion);
            Assert.StartsWith(
                $"%PDF-{majorVersion}.{minorVersion}\r\n%ÿÿÿÿ Created with Melville.Pdf", output);
        }

        [Theory]
        [InlineData(10,7)]
        [InlineData(1,60)]
        public Task ThrowWhenWritingInvalidVersionNumber(byte majorVersion, byte minorVersion)
        {
            var builder = new LowLevelDocumentCreator();
            builder.SetVersion(majorVersion, minorVersion);
            builder.AddRootElement(new PdfDictionary((KnownNames.Type, KnownNames.Catalog)));
            return Assert.ThrowsAsync<ArgumentException>(
                ()=> OutputSimpleDocument(majorVersion, minorVersion));
            
        }

        [Theory]
        [InlineData("Melville.Pdf\n1 0 obj <</Type /Catalog>> endobj")]
        [InlineData("endobj\nxref\n0 2\n0000000000 00000 f\r\n0000000042 00000 n\r\n")]
        [InlineData("n\r\ntrailer\n<</Root 1 0 R /Size 2>>\nstartxref\n76\n%%EOF")]
        public async Task SimpleDocumentContents(string expected) => 
            Assert.Contains(expected, await OutputSimpleDocument());

        [Theory]
        [InlineData("Melville.Pdf\n1 0 obj <</Type /Catalog>> endobj\n3 0 obj <</Type /Page>> endobj")]
        [InlineData("endobj\nxref\n0 4\n0000000002 00000 f\r\n0000000042 00000 n\r\n0000000000 00000 f\r\n0000000076 00000 n\r\n")]
        [InlineData("n\r\ntrailer\n<</Root 1 0 R /Size 4>>\nstartxref\n107\n%%EOF")]
        public async Task TwoItemDocumentContents(string expected) => 
            Assert.Contains(expected, await OutputTwoItemDocument());

        [Theory]
        [InlineData("Melville.Pdf\n1 0 obj <</Type /Catalog>> endobj\n3 0 obj <</Type /Page>> endobj")]
        [InlineData("endobj\n4 0 obj <</Root 1 0 R /Type /XRef /W [1 1 0] /Size 5 /Filter /FlateDecode /DecodeParms <</Predictor 12 /Columns 2>> /Length 29>> stream\r\n")]
        [InlineData("stream\r\nxÚb")]
        public async Task RefStreamContents(string expected) => 
            Assert.Contains(expected, await OutputTwoItemRefStream());
 
        [Theory]
        [InlineData(1, 1, false)]
        [InlineData(1, 4, false)]
        [InlineData(1, 5, true)]
        [InlineData(1, 6, true)]
        [InlineData(2, 0, true)]
        public async Task OnlyWriteRefStreamIfVersionAllows(byte major, byte minor, bool succeed)
        {
            try
            {
                await OutputTwoItemRefStream(major, minor);
                Assert.True(succeed);
            }
            catch (InvalidOperationException)
            {
                Assert.False(succeed);
            }
        }
    }
}