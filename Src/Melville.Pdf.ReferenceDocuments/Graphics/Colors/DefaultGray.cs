using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DefaultGray: ColorBars
{
    public DefaultGray() : base("Default Gray Colorspace")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, KnownNames.DefaultGray,
            CreateColorSpace);
    }

    private PdfObject CreateColorSpace(IPdfObjectCreatorRegistry i)
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        var func = i.Add(builder.Create("{dup 0}"));
        return new PdfArray(
            KnownNames.DeviceN, ColorantNames(), KnownNames.DeviceRGB, func);
    }

    protected virtual PdfArray ColorantNames()
    {
        return new PdfArray(
            NameDirectory.Get("khed")
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpaceAsync(KnownNames.DeviceGray);
        DrawLine(csw);
        csw.SetStrokeColor(.25);
        DrawLine(csw);
        csw.SetStrokeColor(.5);
        DrawLine(csw);
        csw.SetStrokeColor(.75);
        DrawLine(csw);
    }
}