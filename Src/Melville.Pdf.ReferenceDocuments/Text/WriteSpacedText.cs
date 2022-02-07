using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.Text;

public class WriteSpacedText : Card3x5
{
    public WriteSpacedText() : base ("White text using the TJ operator")
    {
    }

    private static readonly PdfName Font1 = NameDirectory.Get("F1"); 
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddStandardFont(Font1, BuiltInFontName.Courier, FontEncodingName.StandardEncoding);
    }

    
    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        using var tr = csw.StartTextBlock();
        await csw.SetStrokeRGB(1.0, 0.0, 0.0);
        await WriteString(csw, tr, Font1, 25);
    }

    private async Task WriteString(ContentStreamWriter csw, ContentStreamWriter.TextBlock tr, PdfName font, int yOffset)
    {
        await csw.SetFont(font, 70);
        tr.SetTextMatrix(1, 0, 0, 1, 30, yOffset);
        tr.ShowSpacedString("A", 500, "B A", -500, "B");
    }
}