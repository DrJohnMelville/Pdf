﻿using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DeviceRGB : ColorBars
{
    public DeviceRGB() : base("Four different Colors from RGB")
    {
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);

        await csw.SetStrokingColorSpaceAsync(ColorSpaceName.DeviceRGB);
        DrawLine(csw);
        csw.SetStrokeColor(1, 0, 0);
        DrawLine(csw);
        csw.SetStrokeColor(0, 1, 0);
        DrawLine(csw);
        csw.SetStrokeColor(0, 0, 1);
        DrawLine(csw);
    }

}