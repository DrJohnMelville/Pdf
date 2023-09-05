using Melville.INPC;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;
using Melville.SharpFont.TrueType;

namespace Melville.Pdf.ReferenceDocuments.Text.CharSet;

public abstract class DisplayCharSet : Card3x5
{
    private readonly BuiltInFontName name;
    private readonly PdfDirectObject fontEncodingName;

    protected DisplayCharSet(BuiltInFontName name) : this(name, FontEncodingName.StandardEncoding) { }

    protected DisplayCharSet(BuiltInFontName name, FontEncodingName fontEncodingName) :
        this(name, (PdfDirectObject)fontEncodingName)
    {
    }

    protected DisplayCharSet(BuiltInFontName name, PdfDirectObject fontEncodingName) :
        base($"All Characters of the {name} Charset")
    {
        this.name = name;
        this.fontEncodingName = fontEncodingName;
    }

    private static readonly PdfDirectObject fontName = PdfDirectObject.CreateName("F1");

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