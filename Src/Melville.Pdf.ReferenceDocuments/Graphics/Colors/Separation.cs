using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class Separation: ColorBars
{
    private readonly PdfName inkName;
    public Separation(PdfName inkName) : base("A separation color space with a fallback function.")
    {
        this.inkName = inkName;
    }

    public Separation() : this(NameDirectory.Get("UnknownInkName"))
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, NameDirectory.Get("CS1"),
            CreateColorSpace);
    }

    private PdfObject CreateColorSpace(ILowLevelDocumentBuilder i)
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        var func = i.Add(builder.Create("{1 exch sub dup 0}"));
        return new PdfArray(
            KnownNames.Separation, inkName, KnownNames.DeviceRGB, func);
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpace(NameDirectory.Get("CS1"));
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
    public SeparationAll() : base(KnownNames.All)
    {
    }
}
public class SeparationNone: Separation
{
    public SeparationNone() : base(KnownNames.None)
    {
    }
}