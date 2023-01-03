using System;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_6IncrementalUpdates
{
    private async Task<PdfLoadedLowLevelDocument> CompositeDocument(Action<IPdfObjectRegistry> create,
        Func<PdfLoadedLowLevelDocument, ILowLevelDocumentModifier, Task> modify)
    {
        var creator = new LowLevelDocumentBuilder();
        create(creator);
        var doc = creator.CreateDocument();
        var stream = new MultiBufferStream();
        await doc.WriteToAsync(stream);

        var pdfLowLevelReader = new PdfLowLevelReader();
        var ld = await pdfLowLevelReader.ReadFromAsync(stream);
        var modifier = ld.Modify();
        await modify(ld, modifier);
        await modifier.WriteModificationTrailer(stream);
            
        return await pdfLowLevelReader.ReadFromAsync(stream);
    }
        
    [Fact]
    public async Task ParseModification()
    {
        var ld2 = await CompositeDocument(creator =>
        {
            var e1 = creator.Add(PdfBoolean.True);
            creator.AddToTrailerDictionary(KnownNames.Root, e1);
            creator.Add(2);
        }, async (ld, modifier) =>
        {
            Assert.Equal("true", (await ld.TrailerDictionary[KnownNames.Root]).ToString());
            Assert.Equal("2", (await ld.Objects[(2, 0)].DirectValueAsync()).ToString());
            modifier.AssignValueToReference((PdfIndirectObject) ld.TrailerDictionary.RawItems[KnownNames.Root],
                PdfBoolean.False);
        });

        Assert.Equal("false", (await ld2.TrailerDictionary[KnownNames.Root]).ToString());
        Assert.Equal("2", (await ld2.Objects[(2,0)].DirectValueAsync()).ToString());

    }

}