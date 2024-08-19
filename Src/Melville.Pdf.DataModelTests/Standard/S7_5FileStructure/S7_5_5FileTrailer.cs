using System;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.ReferenceDocuments.LowLevel;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_5FileTrailer: IDisposable
{
    private IDisposable ctx = RentalPolicyChecker.RentalScope();
    public void Dispose() => ctx.Dispose();
    [Fact]
    public async Task ReadSingleTrailerAsync()
    {
        var doc = await (await MinimalPdfParser.MinimalPdf(1, 7).AsStringAsync()).ParseDocumentAsync(2);
        Assert.Equal(2, doc.TrailerDictionary.Count);
        Assert.Equal(5, (await doc.TrailerDictionary[KnownNames.Size]).Get<int>());
            
    }
   
    [Theory]
    [InlineData("%PDF1.5\r\nHeader tag is wrong.")]
    [InlineData("%PdF-1.5\r\nHeader tag is wrong.")]
    [InlineData("%PDF-1/5\r\nHeader tag is wrong.")]
    [InlineData("%PDF-X.5\r\nHeader tag is wrong.")]
    [InlineData("%PDF-1.X\r\nHeader tag is wrong.")]
    [InlineData("%PDF-1.5\r\nHas no t%railer")]
    [InlineData("%PDF-1.5\r\nHas the word trailer but not a valid dictonary")]
    [InlineData("%PDF-1.5\r\nHas the word trailer")]
    public Task MalformedPDFFilesAsync(string text)
    {
        return Assert.ThrowsAsync<PdfParseException>(() => text.ParseDocumentAsync().AsTask());
    }
}