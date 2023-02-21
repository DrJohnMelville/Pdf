using System;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions;

/// <summary>
/// Unlike most of the PDF costume types, PdfFunction is a class instead of a struct.  This is done for
/// two principle reasons.
/// 1. Type 0, 3, and 4 functions have significant parsing requirements that require intermediate
///    values to be saved.  We do not want to repeat this every time a function is invoked.
/// 2. This allows polymorphism over the function type, which would likely have required a strategy
///    object anyway.
/// PdfFunction handles the input and output mapping that is common to all functions and delegates
/// actual computation to its children.
/// </summary>
public abstract class PdfFunction : IPdfFunction
{
    /// <inheritdoc />
    public ClosedInterval[] Domain { get; }

    /// <inheritdoc />
    public ClosedInterval[] Range { get; }

    private protected PdfFunction(ClosedInterval[] domain, ClosedInterval[] range)
    {
        Domain = domain;
        Range = range;
    }

    /// <inheritdoc />
    public void Compute(in ReadOnlySpan<double> input, in Span<double> result)
    {
        CheckSpanLengths(input.Length, result.Length);
        var clippedInput = OutOfDomain(input) ? 
            Clip(input, stackalloc double[Domain.Length], Domain) : input;
        ComputeOverride(clippedInput, result);
        Clip(result, result, Range);
    }

    private void CheckSpanLengths(int inputLength, int resultLength)
    {
        if (inputLength != Domain.Length)
            throw new ArgumentException("Incorrect number of arguments");
        if (resultLength != Range.Length)
            throw new ArgumentException("Incorrect number of return slots");
    }

    private bool OutOfDomain(in ReadOnlySpan<double> input)
    {
        for (int i = 0; i < Domain.Length; i++)
        {
            if (Domain[i].OutOfInterval(input[i])) return true;
        }
        return false;
    }

    private ReadOnlySpan<double> Clip(in ReadOnlySpan<double> input, in Span<double> output,
        ClosedInterval[] intervals)
    {
        for (int i = 0; i < intervals.Length; i++)
        {
            output[i] = intervals[i].Clip(input[i]);
        }
        return output;
    }

    /// <summary>
    /// When overriden in a descendant class this method computes the result for a given input.
    /// </summary>
    /// <param name="input">The inputs to the function.</param>
    /// <param name="result">Span to which the outputs are written/</param>
    protected abstract void ComputeOverride(in ReadOnlySpan<double> input, in Span<double> result);
}