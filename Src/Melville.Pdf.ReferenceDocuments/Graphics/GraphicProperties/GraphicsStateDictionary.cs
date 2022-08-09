using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.GraphicProperties;

public class GraphicsStateDictionary:Card3x5
{
    private PdfName dictionaryName = NameDirectory.Get("GS1");

    public GraphicsStateDictionary() : base("Line with Style set from GraphicStateDictionary")
    {
        
    }

    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.ExtGState, dictionaryName, new DictionaryBuilder()
            .WithItem(KnownNames.LW, new PdfInteger(25)).AsDictionary());
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.MoveTo(0,0);
        csw.LineTo(300, 300);
        csw.StrokePath();
        await csw.LoadGraphicStateDictionary(dictionaryName);
        csw.MoveTo(0,300);
        csw.LineTo(300,0);
        csw.StrokePath();
    }
}