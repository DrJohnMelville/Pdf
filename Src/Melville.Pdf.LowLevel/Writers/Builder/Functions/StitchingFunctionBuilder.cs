using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder.Functions;

internal readonly struct StitchedFunction
{
    public PdfObject Function { get; }
    public double ExclusiveMaximum { get; }
    public ClosedInterval Encode { get; }

    public StitchedFunction(PdfObject function, double exclusiveMaximum, ClosedInterval encode)
    {
        Function = function;
        ExclusiveMaximum = exclusiveMaximum;
        Encode = encode;
    }
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
    public PdfDictionary Create() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.FunctionType, 3)
            .WithItem(KnownNames.Domain, DomainArray())
            .WithItem(KnownNames.Bounds, BoundsArray())
            .WithItem(KnownNames.Encode, functions.Select(i => i.Encode).AsPdfArray(functions.Count))
            .WithItem(KnownNames.Functions, new PdfArray(functions.Select(i => i.Function)))
            .AsDictionary();

    private PdfArray BoundsArray() =>
        new(functions.Select(i=>(PdfNumber)i.ExclusiveMaximum).SkipLast(1));

    private PdfArray DomainArray() => 
        new(minimum, CurrentMaxInterval());

    private double CurrentMaxInterval() => 
        functions.Select(i=>i.ExclusiveMaximum).DefaultIfEmpty(minimum).Last();

    /// <summary>
    /// Add a subfunction to the stitiched function.
    /// </summary>
    /// <param name="function">PdfObject that defines the function for this interval.</param>
    /// <param name="exclusiveMaximum">The maximum value of this interval</param>
    /// <exception cref="ArgumentException">If the exclusiveMaximum is less than the currently declared maximum.</exception>
    public void AddFunction(PdfDictionary function, double exclusiveMaximum) =>
        AddFunction(function, exclusiveMaximum, (minimum, exclusiveMaximum));

    /// <summary>
    /// Add a subfunction to the stitiched function.
    /// </summary>
    /// <param name="function">PdfObject that defines the function for this interval.</param>
    /// <param name="exclusiveMaximum">The maximum value of this interval</param>
    /// <param name="encode">Encode interval for this segment</param>
    /// <exception cref="ArgumentException">If the exclusiveMaximum is less than the currently declared maximum.</exception>
    public void AddFunction(PdfObject function, double exclusiveMaximum, ClosedInterval encode)
    {
        if (exclusiveMaximum < CurrentMaxInterval())
            throw new ArgumentException("Exclusive maximum must be greater than the prior maximum");
        functions.Add(new StitchedFunction(function, exclusiveMaximum, encode));
    }
}