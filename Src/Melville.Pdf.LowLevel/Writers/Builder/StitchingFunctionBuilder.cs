using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public readonly struct StitchedFunction
{
    public PdfDictionary Function { get; }
    public double ExclusiveMaximum { get; }
    public ClosedInterval Encode { get; }

    public StitchedFunction(PdfDictionary function, double exclusiveMaximum, ClosedInterval encode)
    {
        Function = function;
        ExclusiveMaximum = exclusiveMaximum;
        Encode = encode;
    }
}
public class StitchingFunctionBuilder
{
    private readonly double minimum;
    private readonly List<StitchedFunction> functions = new();

    public StitchingFunctionBuilder(double minimum)
    {
        this.minimum = minimum;
    }

    public PdfDictionary Create() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.FunctionType, new PdfInteger(3))
            .WithItem(KnownNames.Domain, DomainArray())
            .WithItem(KnownNames.Bounds, BoundsArray())
            .WithItem(KnownNames.Encode, functions.Select(i => i.Encode).AsPdfArray(functions.Count))
            .WithItem(KnownNames.Functions, new PdfArray(functions.Select(i => i.Function)))
            .AsDictionary();

    private PdfArray BoundsArray() =>
        new(functions.Select(i=>new PdfDouble(i.ExclusiveMaximum)).SkipLast(1));

    private PdfArray DomainArray() => 
        new(new PdfDouble(minimum), new PdfDouble(CurrentMaxInterval()));

    private double CurrentMaxInterval() => 
        functions.Select(i=>i.ExclusiveMaximum).DefaultIfEmpty(minimum).Last();

    public void AddFunction(PdfDictionary function, double exclusiveMaximum) =>
        AddFunction(function, exclusiveMaximum, (minimum, exclusiveMaximum));

    public void AddFunction(PdfDictionary function, double exclusiveMaximum, ClosedInterval encode)
    {
        if (exclusiveMaximum < CurrentMaxInterval())
            throw new ArgumentException("Exclusive maximum must be greater than the prior maximum");
        functions.Add(new StitchedFunction(function, exclusiveMaximum, encode));
    }
}