using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.GraphicProperties;

public class GraphicsStateDictionary:Card3x5
{
    private PdfDirectObject dictionaryName = PdfDirectObject.CreateName("GS1");

    public GraphicsStateDictionary() : base("Line with Style set from GraphicStateDictionary")
    {
        
    }

    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.ExtGState, dictionaryName, new DictionaryBuilder()
            .WithItem(KnownNames.LW, 25).AsDictionary());
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.MoveTo(0,0);
        csw.LineTo(300, 300);
        csw.StrokePath();
        await csw.LoadGraphicStateDictionaryAsync(dictionaryName);
        csw.MoveTo(0,300);
        csw.LineTo(300,0);
        csw.StrokePath();
    }
}

public class AlphaValueDictionary:Card3x5
{
    private PdfDirectObject dictionaryName = PdfDirectObject.CreateName("GS1");

    public AlphaValueDictionary() : base("Line with Style set from GraphicStateDictionary")
    {
        
    }

    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.ExtGState, dictionaryName, new DictionaryBuilder()
            .WithItem(KnownNames.CA, 0.5)
            .WithItem(KnownNames.ca, 0.25)
            .AsDictionary());
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.MoveTo(0,0);
        csw.LineTo(300, 300);
        csw.StrokePath();
        await csw.LoadGraphicStateDictionaryAsync(dictionaryName);
        csw.SetLineWidth(5);
        await csw.SetNonstrokingRgbAsync(1, 0, 0);
        csw.Rectangle(10,10, 300, 150);
        csw.FillAndStrokePath();
    }
}