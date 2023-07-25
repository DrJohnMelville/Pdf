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
    private async Task<PdfLoadedLowLevelDocument> CompositeDocumentAsync(Action<IPdfObjectCreatorRegistry> create,
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
        await modifier.WriteModificationTrailerAsync(stream);
            
        return await pdfLowLevelReader.ReadFromAsync(stream);
    }
        
    [Fact]
    public async Task ParseModificationAsync()
    {
        var ld2 = await CompositeDocumentAsync(creator =>
        {
            var e1 = creator.Add(true);
            creator.AddToTrailerDictionary(KnownNames.RootTName, e1);
            creator.Add(2);
        }, async (ld, modifier) =>
        {
            Assert.Equal("true", (await ld.TrailerDictionary[KnownNames.RootTName]).ToString());
            Assert.Equal("2", (await ld.Objects[(2, 0)].LoadValueAsync()).ToString());
            modifier.ReplaceReferenceObject(ld.TrailerDictionary.RawItems[KnownNames.RootTName],
                false);
        });

        Assert.Equal("false", (await ld2.TrailerDictionary[KnownNames.RootTName]).ToString());
        Assert.Equal("2", (await ld2.Objects[(2,0)].LoadValueAsync()).ToString());

    }

}