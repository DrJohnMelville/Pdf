using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DeviceCMYK2: ColorBars
{
    public DeviceCMYK2() : base("Four more different Colors from CMYK")
    {
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokeCMYKAsync(.25, .199, .203, 0);
        DrawLine(csw);
        await csw.SetStrokeCMYKAsync(0, 0, 1, 1);
        DrawLine(csw);
        await csw.SetStrokeCMYKAsync(0, .4, .4, 1);
        DrawLine(csw);
        await csw.SetStrokeCMYKAsync(1,1,1,1);
        DrawLine(csw);
    }
}