using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer;

public class LowLevelDocumentModifierTest
{
    private readonly PdfLowLevelDocument baseDoc;

    public LowLevelDocumentModifierTest()
    {
        var builder = new LowLevelDocumentBuilder();
        var rootref = builder.Add(PdfBoolean.True);
        builder.Add(PdfBoolean.False);
        builder.AddToTrailerDictionary(KnownNames.Root, rootref);
        baseDoc = builder.CreateDocument();
    }

    private async Task<PdfLoadedLowLevelDocument> LoadedDocument(PdfLowLevelDocument doc)
    {
        var ms = new MultiBufferStream();
        await doc.WriteToAsync(ms);
        return await new PdfLowLevelReader().ReadFromAsync(ms.CreateReader());
    }

    private async Task DoDocumentModificationTests(
        string expected, Action<PdfLoadedLowLevelDocument, LowLevelDocumentModifier> modifications, 
        PdfLowLevelDocument originalDoc, long offset = 0)
    {
        var target = new TestWriter();
        var doc = await LoadedDocument(originalDoc);
        var sut = new LowLevelDocumentModifier(doc);
        modifications(doc, sut);
        await sut.WriteModificationTrailerAsync(target.Writer, offset);
        Assert.Equal(expected, target.Result());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1234)]
    public Task NullModification(long offset) => 
        DoDocumentModificationTests($"xref\n0 0\ntrailer\n<</Root 1 0 R/Prev 83/Size 2>>\nstartxref\n{offset}\n%%EOF",
            (doc, mod) => { }, baseDoc,
            offset);

    [Fact]
    public Task AddOneObject() =>
        DoDocumentModificationTests($"2 0 obj false endobj\nxref\n2 1\n0000001234 00000 n\r\ntrailer\n<</Root 1 0 R/Prev 83/Size 3>>\nstartxref\n1255\n%%EOF",
            (doc, mod) => mod.Add(PdfBoolean.False), baseDoc,
            1234);
    [Fact]
    public Task ReplaceOneObject() =>
        DoDocumentModificationTests($"1 0 obj false endobj\nxref\n1 1\n0000001234 00000 n\r\ntrailer\n<</Root 1 0 R/Prev 83/Size 2>>\nstartxref\n1255\n%%EOF",
            (doc, mod) => mod.ReplaceReferenceObject(doc.Objects[(1,0)],PdfBoolean.False), baseDoc,
            1234);

    private static PdfLowLevelDocument SixItemDocument()
    {
        var creator = new LowLevelDocumentBuilder();
        creator.Add(0);
        creator.Add(1);
        creator.Add(3);
        creator.Add(3);
        creator.Add(4);
        creator.Add(5);
        creator.Add(6);
        var loaded = creator.CreateDocument();
        return loaded;
    }

    [Fact]
    public Task ReplaceThreeInTwoRuns()
    {
        return DoDocumentModificationTests(
            $"2 0 obj (Two) endobj\n3 0 obj (Three) endobj\n5 0 obj (Five) endobj\nxref\n2 2\n0000005000 00000 n\r\n0000005021 00000 n\r\n5 1\n0000005044 00000 n\r\ntrailer\n<</Prev 161/Size 7>>\nstartxref\n5066\n%%EOF",
            (doc, mod) =>
            {
                mod.ReplaceReferenceObject(doc.Objects[(2,0)], PdfString.CreateAscii("Two"));
                mod.ReplaceReferenceObject(doc.Objects[(3,0)], PdfString.CreateAscii("Three"));
                mod.ReplaceReferenceObject(doc.Objects[(5,0)], PdfString.CreateAscii("Five"));
            }
            , SixItemDocument(), 5000);
    }
}