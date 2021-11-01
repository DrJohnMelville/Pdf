using System;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public delegate double SimpleFunctionResult(ReadOnlySpan<double> input);

public readonly struct SampledFunctionOutput
{
    public ClosedInterval Range { get; }
    public ClosedInterval Decode { get; }
    public SimpleFunctionResult Definition { get; }

    public SampledFunctionOutput(ClosedInterval range, ClosedInterval decode, SimpleFunctionResult definition)
    {
        Range = range;
        Decode = decode;
        Definition = definition;
    }

    public bool DecodeTrivial(int bitsPerSample) =>
        DoubleCompare.WithinOne(0.0, Decode.MinValue) &&
        DoubleCompare.WithinOne((1 << bitsPerSample) - 1, Decode.MaxValue);
}