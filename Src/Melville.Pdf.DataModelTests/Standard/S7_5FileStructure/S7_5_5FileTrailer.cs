using System;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.ReferenceDocuments.LowLevel;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_5FileTrailer
{
    [Test]
    public async Task ReadSingleTrailerAsync()
    {
        using var doc = await (await MinimalPdfParser.MinimalPdf(1, 7).AsStringAsync()).ParseDocumentAsync(2);
        Assert.Equal(2, doc.TrailerDictionary.Count);
        Assert.Equal(5, (await doc.TrailerDictionary[KnownNames.Size]).Get<int>());
            
    }
   
    [Test]
    [Arguments("%PDF1.5\r\nHeader tag is wrong.")]
    [Arguments("%PdF-1.5\r\nHeader tag is wrong.")]
    [Arguments("%PDF-1/5\r\nHeader tag is wrong.")]
    [Arguments("%PDF-X.5\r\nHeader tag is wrong.")]
    [Arguments("%PDF-1.X\r\nHeader tag is wrong.")]
    [Arguments("%PDF-1.5\r\nHas no t%railer")]
    [Arguments("%PDF-1.5\r\nHas the word trailer but not a valid dictonary")]
    [Arguments("%PDF-1.5\r\nHas the word trailer")]
    public Task MalformedPDFFilesAsync(string text)
    {
        return Assert.ThrowsAsync<PdfParseException>(() => text.ParseDocumentAsync().AsTask());
    }
}