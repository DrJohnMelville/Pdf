using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.Text;

public abstract class FontDefinitionTest : Card3x5
{
    protected string TextToRender { get; set; } = "Is Text";
    protected FontDefinitionTest(string helpText) : base (helpText)
    {
    }

    private static readonly PdfName Font1 = NameDirectory.Get("F1"); 
    private static readonly PdfName Font2 = NameDirectory.Get("F2"); 
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddStandardFont(Font1, BuiltInFontName.Courier, FontEncodingName.StandardEncoding);
        page.AddResourceObject(ResourceTypeName.Font, Font2, CreateFont);
    }

    protected abstract PdfObject CreateFont(ILowLevelDocumentCreator arg);
    
    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        using var tr = csw.StartTextBlock();
        await csw.SetStrokeRGB(1.0, 0.0, 0.0);
        await WriteString(csw, tr, Font1, 25);
        await WriteString(csw, tr, Font2, 125);
    }

    private async Task WriteString(ContentStreamWriter csw, ContentStreamWriter.TextBlock tr, PdfName font, int yOffset)
    {
        await csw.SetFont(font, 70);
        tr.SetTextMatrix(1, 0, 0, 1, 30, yOffset);
        tr.ShowString(TextToRender);
    }
}