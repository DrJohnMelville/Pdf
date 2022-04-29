using System;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

public class InvisibleColorSpace: IColorSpace
{
    public DeviceColor SetColor(in ReadOnlySpan<double> newColor) => DeviceColor.Invisible;

    public DeviceColor DefaultColor()  => DeviceColor.Invisible;
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) => DeviceColor.Invisible;
    public int ExpectedComponents { get; }

    public InvisibleColorSpace(int expectedComponents)
    {
        ExpectedComponents = expectedComponents;
    }
    
    public ClosedInterval[] ColorComponentRanges(int bitsPerComponent) =>
      Enumerable.Repeat(new ClosedInterval(0,1), ExpectedComponents).ToArray();

}