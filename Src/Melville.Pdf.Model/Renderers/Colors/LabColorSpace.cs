﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

internal class LabColorSpace : IColorSpace
{
    private readonly DoubleColor whitePoint;
    private readonly ClosedInterval aInterval;
    public readonly ClosedInterval bInterval;

    public LabColorSpace(DoubleColor whitePoint, ClosedInterval aInterval, ClosedInterval bInterval)
    {
        this.whitePoint = whitePoint;
        this.aInterval = aInterval;
        this.bInterval = bInterval;
        outputIntervals = new[]
        {
            new ClosedInterval(0, 100), aInterval, bInterval
        };
    }

    private ClosedInterval[] outputIntervals;
    public ClosedInterval[] ColorComponentRanges(int bitsPerComponent) => outputIntervals;


    public static async ValueTask<IColorSpace> ParseAsync(PdfDictionary parameters)
    {
        var wp = await ReadWhitePointAsync(parameters).CA();
        var array = (await parameters.GetOrNullAsync(KnownNames.Range).CA())
            .TryGet(out PdfArray? arr)
            ? await arr.CastAsync<double>().CA()
            : Array.Empty<double>();
        
        return new LabColorSpace(wp, 
            new ClosedInterval(TryGet(array, 0, - 100), TryGet(array, 1, 100)),
            new ClosedInterval(TryGet(array, 2, - 100), TryGet(array, 3, 100))
            );
    }

    private static async Task<DoubleColor> ReadWhitePointAsync(PdfDictionary parameters)
    {
        var array = await parameters.GetAsync<PdfArray>(KnownNames.WhitePoint).CA();
        return await array.AsDoubleColorAsync().CA();
    }

    private static double TryGet(IReadOnlyList<double> arr, int index, double defaultValue) =>
         arr.Count > index ? arr[index] : defaultValue;

    public DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 3)
            throw new PdfParseException("Wrong number of parameters for Lab color");
        var commonPart = (newColor[0] + 16) / 116;
        var L = commonPart + (aInterval.Clip(newColor[1]) / 500);
        var M = commonPart;
        var N = commonPart - (bInterval.Clip(newColor[2]) / 200);
        return XyzToRgbTransformFactory.Create(whitePoint).ToDeviceColor(stackalloc float[]
        {
            (float)(whitePoint.Red * GFunc(L)),
            (float)(whitePoint.Green * GFunc(M)),
            (float)(whitePoint.Blue * GFunc(N))
        });
    }

    public double GFunc(double x) =>
        x >= (6.0 / 29) ? 
            x * x * x :
            (108.0 / 841) * (x - (4.0 / 29));
    
    public DeviceColor DefaultColor() => DeviceColor.Black;

    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor)
    {
        var sourceInterval = new ClosedInterval(0, 255);
        return SetColor(stackalloc double[]
        {
            newColor[0] * 100.0 / 255.0,
            sourceInterval.MapTo(aInterval, newColor[1]),
            sourceInterval.MapTo(bInterval, newColor[2]),
        });
    }
    
    public int ExpectedComponents => 3;
}