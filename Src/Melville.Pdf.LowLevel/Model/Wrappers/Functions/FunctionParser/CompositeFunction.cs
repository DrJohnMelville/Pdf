using System;
using System.Diagnostics;
using System.Linq;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

public class CompositeFunction : IPdfFunction
{
    private readonly IPdfFunction[] innerFunctions;

    public CompositeFunction(IPdfFunction[] innerFunctions)
    {
        Debug.Assert(innerFunctions.Length > 1); // zero is an error and 1 is just inefficient.
        this.innerFunctions = innerFunctions;
        Range = innerFunctions.SelectMany(i => i.Range).ToArray();
    }

    public ClosedInterval[] Domain => innerFunctions[0].Domain;
    public ClosedInterval[] Range { get; }
    public void Compute(in ReadOnlySpan<double> input, in Span<double> result)
    {
        var position = 0;
        foreach (var innerFunction in innerFunctions)
        {
            VerifyThatAllFunctionsHaveSameDomain(innerFunction);
            var expectedResults = innerFunction.Range.Length;
            innerFunction.Compute(input, result.Slice(position, expectedResults));
            position += expectedResults;
        }
    }

    [Conditional("DEBUG")]
    private void VerifyThatAllFunctionsHaveSameDomain(IPdfFunction innerFunction)
    {
        Debug.Assert(innerFunction.Domain.Length == innerFunctions[0].Domain.Length);
        Debug.Assert(innerFunction.Domain.Zip(innerFunctions[0].Domain, (i, j) => i == j).All(i => i));
    }
}