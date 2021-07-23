using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.DataModelTests.PdfStreamHolderTest;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer
{
    public class LowLevelDocumentModifierTest
    {
        private readonly PdfLowLevelDocument baseDoc;

        public LowLevelDocumentModifierTest()
        {
            var builder = new LowLevelDocumentCreator();
            var rootref = builder.Add(PdfBoolean.True);
            builder.Add(PdfBoolean.False);
            builder.AddToTrailerDictionary(KnownNames.Root, rootref);
            baseDoc = builder.CreateDocument();
        }

        private async Task<PdfLoadedLowLevelDocument> LoadedDocument(PdfLowLevelDocument doc)
        {
            var ms = new MemoryStream();
            await doc.WriteTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return await RandomAccessFileParser.Parse(ms);
        }

        private async Task DoDocumentModificationTests(
            string expected, Action<PdfLoadedLowLevelDocument, LowLevelDocumentModifier> modifications, 
            PdfLowLevelDocument originalDoc, long offset = 0)
        {
            var target = new TestWriter();
            var doc = await LoadedDocument(originalDoc);
            var sut = new LowLevelDocumentModifier(doc);
            modifications(doc, sut);
            await sut.WriteModificationTrailer(target.Writer, offset);
            Assert.Equal(expected, target.Result());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1234)]
        public Task NullModification(long offset) => 
            DoDocumentModificationTests($"xref\n0 0\ntrailer\n<</Root 1 0 R /Size 2 /Prev 83>>\nstartxref\n{offset}\n%%EOF",
                (doc, mod) => { }, baseDoc,
                offset);

        [Fact]
        public Task DeleteOneObject() =>
            DoDocumentModificationTests($"xref\n0 2\n0000000001 65535 f\r\n0000000000 00000 f\r\ntrailer\n<</Root 1 0 R /Size 2 /Prev 83>>\nstartxref\n1234\n%%EOF",
                (doc, mod) => mod.DeleteObject(doc.Objects.Values.First()), baseDoc,
                1234);
        [Fact]
        public Task AddOneObject() =>
            DoDocumentModificationTests($"2 0 obj false endobj\nxref\n2 1\n0000001234 00000 n\r\ntrailer\n<</Root 1 0 R /Size 3 /Prev 83>>\nstartxref\n1255\n%%EOF",
                (doc, mod) => mod.Add(PdfBoolean.False), baseDoc,
                1234);
        [Fact]
        public Task ReplaceOneObject() =>
            DoDocumentModificationTests($"1 0 obj false endobj\nxref\n1 1\n0000001234 00000 n\r\ntrailer\n<</Root 1 0 R /Size 2 /Prev 83>>\nstartxref\n1255\n%%EOF",
                (doc, mod) => mod.AssignValueToReference(doc.Objects.Values.First(),PdfBoolean.False), baseDoc,
                1234);

        private static PdfLowLevelDocument SixItemDocument()
        {
            var creator = new LowLevelDocumentCreator();
            creator.Add(new PdfInteger(0));
            creator.Add(new PdfInteger(1));
            creator.Add(new PdfInteger(3));
            creator.Add(new PdfInteger(3));
            creator.Add(new PdfInteger(4));
            creator.Add(new PdfInteger(5));
            creator.Add(new PdfInteger(6));
            var loaded = creator.CreateDocument();
            return loaded;
        }

        [Fact]
        public Task ReplaceThreeInTwoRuns()
        {
            return DoDocumentModificationTests(
                $"2 0 obj (Two) endobj\n3 0 obj (Three) endobj\n5 0 obj (Five) endobj\nxref\n2 2\n0000005000 00000 n\r\n0000005021 00000 n\r\n5 1\n0000005044 00000 n\r\ntrailer\n<</Size 7 /Prev 161>>\nstartxref\n5066\n%%EOF",
                (doc, mod) =>
                {
                    mod.AssignValueToReference(doc.Objects[(2,0)], new PdfString("Two"));
                    mod.AssignValueToReference(doc.Objects[(3,0)], new PdfString("Three"));
                    mod.AssignValueToReference(doc.Objects[(5,0)], new PdfString("Five"));
                }
                , SixItemDocument(), 5000);
        }
        [Fact]
        public Task DeleteThreeInTwoRuns()
        {
            return DoDocumentModificationTests(
                $"xref\n0 1\n0000000005 65535 f\r\n2 2\n0000000000 00000 f\r\n0000000002 00000 f\r\n5 1\n0000000003 00000 f\r\ntrailer\n<</Size 7 /Prev 161>>\nstartxref\n5000\n%%EOF",
                (doc, mod) =>
                {
                    mod.DeleteObject(doc.Objects[(2,0)]);
                    mod.DeleteObject(doc.Objects[(3,0)]);
                    mod.DeleteObject(doc.Objects[(5,0)]);
                }
                , SixItemDocument(), 5000);
        }
    }
}