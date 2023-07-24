using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DeviceN: ColorBars
{
    public DeviceN() : base("A DeviceN colorspace with a fallbackFunction.")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, PdfDirectValue.CreateName("CS1"),
            CreateColorSpace);
    }

    private PdfIndirectValue CreateColorSpace(IPdfObjectCreatorRegistry i)
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        var func = i.Add(builder.Create("{exch}"));
        return new PdfValueArray(
            KnownNames.DeviceNTName, ColorantNames(), KnownNames.DeviceRGBTName, func);
    }

    protected virtual PdfValueArray ColorantNames()
    {
        return new PdfValueArray(
            PdfDirectValue.CreateName("khed"),
            PdfDirectValue.CreateName("QGR"),
            PdfDirectValue.CreateName("DFS")
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpaceAsync(PdfDirectValue.CreateName("CS1"));
        DrawLine(csw);
        csw.SetStrokeColor(1, 0, 0);
        DrawLine(csw);
        csw.SetStrokeColor(0,1,0);
        DrawLine(csw);
        csw.SetStrokeColor(0, 0, 1);
        DrawLine(csw);
    }
}

public class DeviceNNone : DeviceN
{
    public DeviceNNone():base()
    {
    }

    protected override PdfValueArray ColorantNames() => new PdfValueArray(
        KnownNames.NoneTName, KnownNames.NoneTName, KnownNames.NoneTName
    );
}
