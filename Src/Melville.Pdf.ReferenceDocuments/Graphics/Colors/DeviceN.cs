using Melville.Pdf.LowLevel.Model.ContentStreams;
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
        page.AddResourceObject(ResourceTypeName.ColorSpace, NameDirectory.Get("CS1"),
            CreateColorSpace);
    }

    private PdfObject CreateColorSpace(ILowLevelDocumentCreator i)
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        var func = i.Add(builder.Create("{exch}"));
        return new PdfArray(
            KnownNames.DeviceN, ColorantNames(), KnownNames.DeviceRGB, func);
    }

    protected virtual PdfArray ColorantNames()
    {
        return new PdfArray(
            NameDirectory.Get("khed"),
            NameDirectory.Get("QGR"),
            NameDirectory.Get("DFS")
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpace(NameDirectory.Get("CS1"));
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

    protected override PdfArray ColorantNames() => new PdfArray(
        KnownNames.None, KnownNames.None, KnownNames.None
    );
}
