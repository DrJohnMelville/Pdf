using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DeviceCMYK: ColorBars
{
    public DeviceCMYK() : base("Four different Colors from CMYK")
    {
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpace(ColorSpaceName.DeviceCMYK);
        DrawLine(csw);
        csw.SetStrokeColor(1,0,0,0);
        DrawLine(csw);
        csw.SetStrokeColor(0,1,0,0);
        DrawLine(csw);
        csw.SetStrokeColor(0,0,1,0);
        DrawLine(csw);
    }
}