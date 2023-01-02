using System;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions;

internal readonly struct StitchedFunctionSegment
{
    private readonly ClosedInterval domain;
    private readonly ClosedInterval encode;
    private readonly IPdfFunction innerFunction;

    public StitchedFunctionSegment(ClosedInterval domain, ClosedInterval encode, IPdfFunction innerFunction)
    {
        this.domain = domain;
        this.encode = encode;
        this.innerFunction = innerFunction;
    }

    public int NumberOfOutputs => innerFunction.Range.Length;

    public bool TryCompute(in ReadOnlySpan<double> input, in Span<double> result)
    {
        if (FunctionAppliesToInput(input)) return false;
        EvaluateInnerFunction(input, result);
        return true;
    }

    // we check the segments in ascending order, so we do not need to check the lower bound.
    private bool FunctionAppliesToInput(ReadOnlySpan<double> input) => input[0] >= domain.MaxValue;

    private void EvaluateInnerFunction(ReadOnlySpan<double> input, Span<double> result) => 
        innerFunction.Compute(domain.MapTo(encode, input[0]), result);
}

internal sealed class StitchedFunction:PdfFunction
{
    private StitchedFunctionSegment[] segments;
    public StitchedFunction(
        ClosedInterval[] domain, ClosedInterval[] range, StitchedFunctionSegment[] segments) : base(domain, range)
    {
        this.segments = segments;
    }

    protected override void ComputeOverride(in ReadOnlySpan<double> input, in Span<double> result)
    {
        foreach (var segment in segments)
        {
            if (segment.TryCompute(input, result)) return;
        }
        // this could happen if the function is evaluated at the maximum value of the domain.
        segments[^1].TryCompute(input, result);
    }
}