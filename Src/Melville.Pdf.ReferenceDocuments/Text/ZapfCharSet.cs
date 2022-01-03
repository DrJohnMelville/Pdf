using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.Text;

public class ZapfCharSet : DisplayCharSet
{
    public ZapfCharSet() : base(BuiltInFontName.ZapfDingbats)
    {
    }
}

public class SymbolCharSer : DisplayCharSet
{
    public SymbolCharSer() : base(BuiltInFontName.Symbol)
    {
    }
}
public abstract class DisplayCharSet : Card3x5
{
    private BuiltInFontName name;

    protected DisplayCharSet(BuiltInFontName name) : base ($"All Characters of the {name} Charset")
    {
        this.name = name;
    }

    private static readonly PdfName fontName = NameDirectory.Get("F1"); 
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddStandardFont(fontName, name, FontEncodingName.StandardEncoding);
        
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        using (var tr = csw.StartTextBlock())
        {
            tr.SetTextMatrix(1,0,0,1,30,224);
            csw.SetTextLeading(12);
            await csw.SetFont(fontName, 12);
            for (int i = 0; i < 16; i++) 
            {
                tr.MoveToNextLineAndShowString(Enumerable.Range(0,16).Select(j=>(byte)(16*i + j)).ToArray());
            }
        }
    }
}