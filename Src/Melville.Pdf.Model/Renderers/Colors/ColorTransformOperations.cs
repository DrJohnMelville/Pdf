﻿using System;
using System.Diagnostics;
using Melville.Icc.ColorTransforms;

namespace Melville.Pdf.Model.Renderers.Colors;

internal static class ColorTransformOperations
{
    public static DeviceColor ToDeviceColor(this IColorTransform xform, in ReadOnlySpan<float> input)
    {
        Debug.Assert(xform.Outputs == 3);
        Span<float> intermed = stackalloc float[3];
        xform.Transform(input, intermed);
        return DeviceColor.FromDoubles(
            intermed[0],
            intermed[1],
            intermed[2]
        );
    }
}