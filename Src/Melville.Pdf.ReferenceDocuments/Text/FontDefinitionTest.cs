using System.ComponentModel.Design;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.Text;

public abstract class FontDefinitionTest : Card3x5
{
    protected string TextToRender { get; set; } = "Is Text";
    protected double FontSize { get; set; } = 70;
    protected FontDefinitionTest(string helpText) : base (helpText)
    {
    }

    private static readonly PdfDirectValue Font1 = PdfDirectValue.CreateName("F1"); 
    private static readonly PdfDirectValue Font2 = PdfDirectValue.CreateName("F2"); 
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddStandardFont(Font1, BuiltInFontName.Courier, FontEncodingName.StandardEncoding);
        page.AddResourceObject(ResourceTypeName.Font, Font2, i=>CreateFont(i));
    }

    protected abstract PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg);
    
    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        using var tr = csw.StartTextBlock();
        await csw.SetStrokeRGBAsync(1.0, 0.0, 0.0);
        await WriteStringAsync(csw, tr, Font1, 25);
        await BetweenFontWritesAsync(csw);
        await WriteStringAsync(csw, tr, Font2, 125);
    }

    protected virtual ValueTask BetweenFontWritesAsync(ContentStreamWriter csw)
    {
        return ValueTask.CompletedTask;
    }

    private async Task WriteStringAsync(ContentStreamWriter csw, TextBlockWriter tr, PdfDirectValue font, int yOffset)
    {
        await csw.SetFontAsync(font, FontSize);
        tr.SetTextMatrix(1, 0, 0, 1, 30, yOffset);
        await tr.ShowStringAsync(TextToRender);
    }
}