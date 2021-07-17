using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer
{
    public class FileWriterTest
    {
        private async Task<string> Write(PdfLowLevelDocument doc)
        {
            var target = new MemoryStream();
            var writer = new LowLevelDocumentWriter(PipeWriter.Create(target));
            await writer.WriteAsync(doc);
            return target.ToArray().ExtendedAsciiString();
        }

        private async Task<string> OutputSimpleDocument(byte majorVersion = 1, byte minorVersion = 7)
        {
            var builder = new LowLevelDocumentBuilder();
            builder.SetVersion(majorVersion, minorVersion);
            builder.AddRootElement(builder.NewDictionary((KnownNames.Type, KnownNames.Catalog)));
            return await Write(builder.CreateDocument());
        }
        private async Task<string> OutputTwoItemDocument(byte majorVersion = 1, byte minorVersion = 7)
        {
            var builder = new LowLevelDocumentBuilder();
            builder.SetVersion(majorVersion, minorVersion);
            builder.AddRootElement(builder.NewDictionary((KnownNames.Type, KnownNames.Catalog)));
            builder.AsIndirectReference(PdfBoolean.True); // includes a dead object to be skipped
            builder.Add(builder.NewDictionary((KnownNames.Type, KnownNames.Page)));
            return await Write(builder.CreateDocument());
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
            var builder = new LowLevelDocumentBuilder();
            builder.SetVersion(majorVersion, minorVersion);
            builder.AddRootElement(builder.NewDictionary((KnownNames.Type, KnownNames.Catalog)));
            return Assert.ThrowsAsync<ArgumentException>(
                ()=> OutputSimpleDocument(majorVersion, minorVersion));
            
        }

        [Fact]
        public async Task OutputsFirstObject()
        {
            var output = await OutputSimpleDocument();
            Assert.Contains("Melville.Pdf\r\n1 0 obj <</Type /Catalog>> endobj", output);
        }
        [Fact]
        public async Task TwoOutputTowItemFile()
        {
            var output = await OutputTwoItemDocument();
            Assert.Contains("Melville.Pdf\r\n1 0 obj <</Type /Catalog>> endobj\r\n3 0 obj <</Type /Page>> endobj", output);
        }
        [Fact]
        public async Task OutputsXRefTable()
        {
            var output = await OutputSimpleDocument();
            Assert.Contains("endobj\r\nxref\r\n0 2\r\n0000000000 65535 f\r\n0000000043 00000 n\r\n", output);
        }

        [Fact]
        public async Task OutputXrefWithSkippedItems()
        {
            var output = await OutputTwoItemDocument();
            Assert.Contains("endobj\r\nxref\r\n0 4\r\n0000000002 65535 f\r\n0000000043 00000 n\r\n0000000000 00000 f\r\n0000000078 00000 n\r\n", output);
        }

        [Fact]
        public async Task OutputsTrailer()
        {
            var output = await OutputSimpleDocument();
            Assert.Contains("n\r\ntrailer\r\n<</Root 1 0 R /Size 2>>\r\nstartxref\r\n78\r\n%%EOF", output);
        }
    }
}