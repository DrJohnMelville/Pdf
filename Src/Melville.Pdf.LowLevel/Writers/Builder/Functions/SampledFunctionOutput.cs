using System;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder.Functions;

public delegate double SimpleFunctionResult(ReadOnlySpan<double> input);

internal readonly struct SampledFunctionOutput
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
        DoubleCompare.WithinOne(Range.MinValue, Decode.MinValue) &&
        DoubleCompare.WithinOne(Range.MaxValue, Decode.MaxValue);
}