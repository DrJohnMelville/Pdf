using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public class S7_5_7ObjectStreams
    {
        [Theory]
        [InlineData("/Type/ObjStm")]
        [InlineData("/N 2")]
        [InlineData("/First 8")]
        [InlineData("1 0 2 6 (One)\n(Two)")]
        public async Task RenderObjectStrem(string expected)
        {
            Assert.Contains(expected, await DocWithObjectStream());
        }

        [Fact]
        public async Task MaxObjectInObjStream()
        {
            Assert.Contains("/Length 21", await DocWithObjectStreamWithHighObjectNumber());
        }

        [Fact]
        public async Task RoundTrip()
        {
            var doc = await (await DocWithObjectStream()).ParseDocumentAsync();
            Assert.Equal("One", (await doc.Objects[(1,0)].DirectValue()).ToString());
            
        }

        [Fact]
        public Task CannotPutStreamInObjectStream()
        {
            var creator = new LowLevelDocumentCreator();
            return Assert.ThrowsAsync<InvalidOperationException>(() =>
                creator.NewObjectStream(creator.AsIndirectReference(
                    creator.NewStream("Hello"))).AsTask()
            );
        }
        [Fact]
        public Task CannotPutNonZeroGenerationStream()
        {
            var creator = new LowLevelDocumentCreator();
            return Assert.ThrowsAsync<InvalidOperationException>(() =>
                creator.NewObjectStream(new PdfIndirectReference(new PdfIndirectObject(12,1, KnownNames.All))).AsTask()
            );
        }
        
        private static async Task<string> DocWithObjectStream()
        {
            var builder = new LowLevelDocumentCreator();
            builder.Add(await builder.NewObjectStream( new []{
            builder.AsIndirectReference(new PdfString("One")),
                builder.AsIndirectReference(new PdfString("Two"))
            }, PdfTokenValues.Null));
            var fileAsString = await DocCreatorToString(builder);
            return fileAsString;
        }
        private static async Task<string> DocWithObjectStreamWithHighObjectNumber()
        {
            var builder = new LowLevelDocumentCreator();
            builder.Add(await builder.NewObjectStream( new []{
            builder.AsIndirectReference(new PdfString("One")),
                new PdfIndirectReference(new PdfIndirectObject(20,0, new PdfString("Two")))
            }, PdfTokenValues.Null));
            var fileAsString = await DocCreatorToString(builder);
            return fileAsString;
        }

        private static async Task<string> DocCreatorToString(LowLevelDocumentCreator builder)
        {
            var ms = new MultiBufferStream();
            var writer = new LowLevelDocumentWriter(PipeWriter.Create(ms), builder.CreateDocument());
            await writer.WriteWithReferenceStream();
            var fileAsString = ms.CreateReader().ReadToArray().ExtendedAsciiString();
            return fileAsString;
        }

        [Fact]
        public async Task ExtractIncludedObjectReferences()
        {
            var builder = new LowLevelDocumentCreator();
            var str = await builder.NewObjectStream( new []{
                builder.AsIndirectReference(new PdfString("One")),
                builder.AsIndirectReference(new PdfString("Two"))
            });

            var output = await str.GetIncludedObjectNumbersAsync();
            Assert.Equal(1, output[0].ObjectNumber);
            Assert.Equal(2, output[1].ObjectNumber);
        }
    }
}