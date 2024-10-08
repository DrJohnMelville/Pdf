﻿using System;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_6IncrementalUpdates
{
    private async Task<PdfLoadedLowLevelDocument> CompositeDocumentAsync(
        Action<IPdfObjectCreatorRegistry> create,
        Func<PdfLoadedLowLevelDocument, ILowLevelDocumentModifier, Task> modify)
    {
        var creator = new LowLevelDocumentBuilder();
        create(creator);
        var doc = creator.CreateDocument(); 
        var source = WritableBuffer.Create();
        await using var writer = source.WritingStream();
        await doc.WriteToAsync(writer);


        var pdfLowLevelReader = new PdfLowLevelReader();
        var ld = await pdfLowLevelReader.ReadFromAsync(source);
        var modifier = ld.Modify();
        await modify(ld, modifier);
        await modifier.WriteModificationTrailerAsync(writer);
            
        return await pdfLowLevelReader.ReadFromAsync(source);
    }
        
    [Fact]
    public async Task ParseModificationAsync()
    {
        using var ld2 = await CompositeDocumentAsync(creator =>
        {
            var e1 = creator.Add(true);
            creator.AddToTrailerDictionary(KnownNames.Root, e1);
            creator.Add(2);
        }, async (ld, modifier) =>
        {
            Assert.Equal("true", (await ld.TrailerDictionary[KnownNames.Root]).ToString());
            Assert.Equal("2", (await ld.Objects[(2, 0)].LoadValueAsync()).ToString());
            modifier.ReplaceReferenceObject(ld.TrailerDictionary.RawItems[KnownNames.Root],
                false);
        });


        var root = await ld2.TrailerDictionary[KnownNames.Root];
        Assert.Equal("false", root.ToString());
        Assert.Equal("2", (await ld2.Objects[(2,0)].LoadValueAsync()).ToString());

    }

}