using System;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder.Functions;

public readonly struct SampledFunctionInput
{
    public ClosedInterval Domain { get; }
    public ClosedInterval Encode { get; }
    public int Sammples { get; }

    public SampledFunctionInput(ClosedInterval domain, ClosedInterval encode, int sammples)
    {
        Domain = domain;
        Encode = encode;
        Sammples = sammples;
    }

    public bool EncodeTrivial() =>
        DoubleCompare.WithinOne(Encode.MinValue, 0) &&
        DoubleCompare.WithinOne(Encode.MaxValue, Sammples - 1);

    public double InputAtSampleLocation(int sample) =>
        Encode.MapTo(Domain, sample);
}

public static class DoubleCompare
{
    public static bool WithinOne(double a, double b) => Math.Abs(a - b) < 0.99;
}