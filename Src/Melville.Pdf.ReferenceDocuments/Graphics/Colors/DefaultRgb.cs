using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DefaultRgb: ColorBars
{
    public DefaultRgb() : base("Default RGB colorspace")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, KnownNames.DefaultRGBTName,
            CreateColorSpace);
    }

    private PdfIndirectObject CreateColorSpace(IPdfObjectCreatorRegistry i)
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddOutput((0, 1));
        var func = i.Add(builder.Create("{0.2 mul exch 0.4 mul add exch 0.2 mul add}"));
        return new PdfArray(
            KnownNames.DeviceNTName, ColorantNames(), KnownNames.DeviceGrayTName, func);
    }

    protected virtual PdfArray ColorantNames()
    {
        return new PdfArray(
            PdfDirectObject.CreateName("khed"),
            PdfDirectObject.CreateName("QGR"),
            PdfDirectObject.CreateName("DFS")
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpaceAsync(KnownNames.DeviceRGBTName);
        DrawLine(csw);
        csw.SetStrokeColor(1, 0, 0);
        DrawLine(csw);
        csw.SetStrokeColor(0,1,0);
        DrawLine(csw);
        csw.SetStrokeColor(0, 0, 1);
        DrawLine(csw);
    }
}