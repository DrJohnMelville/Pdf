using System;
using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.LowLevel.Writers.Builder.Functions;

internal readonly partial struct StitchedFunction
{
    [FromConstructor] public PdfIndirectValue Function { get; }
    [FromConstructor] public double ExclusiveMaximum { get; }
    [FromConstructor] public ClosedInterval Encode { get; }

}

/// <summary>
/// This builder is used to construct a stitched function
/// </summary>
public readonly struct StitchingFunctionBuilder
{
    private readonly double minimum;
    private readonly List<StitchedFunction> functions = new();

    /// <summary>
    /// Create a stitched builder.
    /// </summary>
    /// <param name="minimum">The minimum value for the first domain.</param>
    public StitchingFunctionBuilder(double minimum)
    {
        this.minimum = minimum;
    }

    /// <summary>
    /// Create a PdfDictionary that defines this function.
    /// </summary>
    /// <returns>A pdfdictionary that declares this function.</returns>
    public PdfValueDictionary Create() =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.FunctionTypeTName, 3)
            .WithItem(KnownNames.DomainTName, DomainArray())
            .WithItem(KnownNames.BoundsTName, BoundsArray())
            .WithItem(KnownNames.EncodeTName, functions.Select(i => i.Encode).AsPdfArray(functions.Count))
            .WithItem(KnownNames.FunctionsTName, new PdfValueArray(functions.Select(i => i.Function).ToArray()))
            .AsDictionary();

    private PdfValueArray BoundsArray() =>
        new(functions.Select(i=>(PdfIndirectValue)i.ExclusiveMaximum).SkipLast(1).ToArray());

    private PdfValueArray DomainArray() => 
        new(minimum, CurrentMaxInterval());

    private double CurrentMaxInterval() => 
        functions.Select(i=>i.ExclusiveMaximum).DefaultIfEmpty(minimum).Last();

    /// <summary>
    /// Add a subfunction to the stitiched function.
    /// </summary>
    /// <param name="function">PdfObject that defines the function for this interval.</param>
    /// <param name="exclusiveMaximum">The maximum value of this interval</param>
    /// <exception cref="ArgumentException">If the exclusiveMaximum is less than the currently declared maximum.</exception>
    public void AddFunction(PdfValueDictionary function, double exclusiveMaximum) =>
        AddFunction(function, exclusiveMaximum, (minimum, exclusiveMaximum));

    /// <summary>
    /// Add a subfunction to the stitiched function.
    /// </summary>
    /// <param name="function">PdfObject that defines the function for this interval.</param>
    /// <param name="exclusiveMaximum">The maximum value of this interval</param>
    /// <param name="encode">Encode interval for this segment</param>
    /// <exception cref="ArgumentException">If the exclusiveMaximum is less than the currently declared maximum.</exception>
    public void AddFunction(PdfIndirectValue function, double exclusiveMaximum, ClosedInterval encode)
    {
        if (exclusiveMaximum < CurrentMaxInterval())
            throw new ArgumentException("Exclusive maximum must be greater than the prior maximum");
        functions.Add(new StitchedFunction(function, exclusiveMaximum, encode));
    }
}