using System;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.FontLibrary;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Melville.Pdf.ReferenceDocuments.Text.TrueType;
using Melville.Pdf.TextExtractor;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_10_TextExtraction;

public class TextExtractionIntegrationTest: RentalPolicyTestBase
{
    [Fact]
    public async Task SimpleRenderingAsync()
    {
        using var ren = await new EmbeddedTrueType().AsDocumentRendererAsync(SelfContainedDefaultFonts.Instance);
        var text = await ren.PageTextAsync(1);
        Assert.Equal("Is Text\r\nIs Text", text);
    }
}