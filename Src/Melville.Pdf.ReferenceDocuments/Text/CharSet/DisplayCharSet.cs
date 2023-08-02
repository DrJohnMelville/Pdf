using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.Text.CharSet;

public abstract class DisplayCharSet : Card3x5
{
    private readonly BuiltInFontName name;
    private readonly PdfDirectValue fontEncodingName;

    protected DisplayCharSet(BuiltInFontName name) : this(name, FontEncodingName.StandardEncoding) { }

    protected DisplayCharSet(BuiltInFontName name, FontEncodingName fontEncodingName) :
        this(name, (PdfDirectValue)fontEncodingName)
    {
    }

    protected DisplayCharSet(BuiltInFontName name, PdfDirectValue fontEncodingName) :
        base($"All Characters of the {name} Charset")
    {
        this.name = name;
        this.fontEncodingName = fontEncodingName;
    }

    private static readonly PdfDirectValue fontName = PdfDirectValue.CreateName("F1");

    protected override void SetPageProperties(PageCreator page)
    {
        page.AddStandardFont(fontName, name, fontEncodingName);
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        using (var tr = csw.StartTextBlock())
        {
            tr.SetTextMatrix(1, 0, 0, 1, 30, 224);
            csw.SetTextLeading(12);
            await csw.SetFontAsync(fontName, 12);
            for (int i = 0; i < 16; i++)
            {
                await tr.MoveToNextLineAndShowStringAsync(Enumerable.Range(0, 16).Select(j => (byte)(16 * i + j)).ToArray());
            }
        }
    }
}