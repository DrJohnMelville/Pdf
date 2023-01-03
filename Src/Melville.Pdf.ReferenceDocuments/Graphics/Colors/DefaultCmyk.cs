using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DefaultCmyk: ColorBars
{
    public DefaultCmyk() : base("Default CMYK colorspace")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, KnownNames.DefaultCMYK,
            CreateColorSpace);
    }

    private PdfObject CreateColorSpace(ILowLevelDocumentBuilder i)
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        var func = i.Add(builder.Create("{pop 0.2 mul exch 0.4 mul add exch 0.2 mul add dup dup}"));
        return new PdfArray(
            KnownNames.DeviceN, ColorantNames(), KnownNames.DeviceRGB, func);
    }

    protected virtual PdfArray ColorantNames()
    {
        return new PdfArray(
            NameDirectory.Get("khed"),
            NameDirectory.Get("QGR"),
            NameDirectory.Get("DFS"),
            NameDirectory.Get("DFS")
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpace(KnownNames.DeviceCMYK);
        DrawLine(csw);
        csw.SetStrokeColor(1, 0, 0, .25);
        DrawLine(csw);
        csw.SetStrokeColor(0,1,0, .25);
        DrawLine(csw);
        csw.SetStrokeColor(0, 0, 1, .5);
        DrawLine(csw);
    }
}