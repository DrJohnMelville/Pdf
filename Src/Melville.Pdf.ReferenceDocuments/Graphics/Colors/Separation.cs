using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class Separation: ColorBars
{
    private readonly PdfDirectValue inkName;
    public Separation(PdfDirectValue inkName) : base("A separation color space with a fallback function.")
    {
        this.inkName = inkName;
    }

    public Separation() : this(PdfDirectValue.CreateName("UnknownInkName"))
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
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        var func = i.Add(builder.Create("{1 exch sub dup 0}"));
        return new PdfValueArray(
            KnownNames.SeparationTName, inkName, KnownNames.DeviceRGBTName, func);
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpaceAsync(PdfDirectValue.CreateName("CS1"));
        DrawLine(csw);
        csw.SetStrokeColor(0.25);
        DrawLine(csw);
        csw.SetStrokeColor(0.5);
        DrawLine(csw);
        csw.SetStrokeColor(0.75);
        DrawLine(csw);
    }
}

public class SeparationAll: Separation
{
    public SeparationAll() : base(KnownNames.AllTName)
    {
    }
}
public class SeparationNone: Separation
{
    public SeparationNone() : base(KnownNames.NoneTName)
    {
    }
}