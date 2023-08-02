using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder.Functions;

internal readonly struct ExponentialInterval
{
    public ClosedInterval Bounds { get; }
    public ClosedInterval Range { get; }

    public ExponentialInterval(ClosedInterval bounds, ClosedInterval range)
    {
        Bounds = bounds;
        Range = range;
    }
}

/// <summary>
/// Exponential interpolation functions are defined in 7.10.3 of the pdf 2.0 spec.
/// An exponential function is a 1-> n fucnction where each of the n outputs is the
/// function result 0-1 scaled to various values.
/// </summary>
public readonly struct ExponentialFunctionBuilder
{
    private readonly ClosedInterval domain;
    private readonly double exponent;
    private readonly List<ExponentialInterval> mappings = new();

    /// <summary>
    /// Create an exponential function builder
    /// </summary>
    /// <param name="exponent">Exponent for the interpolation function</param>
    /// <param name="domain">The domain of the function</param>
    public ExponentialFunctionBuilder(double exponent, ClosedInterval? domain = null)
    {
        this.exponent = exponent;
        this.domain = domain ?? new ClosedInterval(0,1);
    }

    /// <summary>
    /// Render the described function as a Pdf Dictionary.
    /// </summary>
    /// <returns>A PdfDictionary declaring this function</returns>
    public PdfDictionary Create() => DictionaryItems().AsDictionary();

    private DictionaryBuilder DictionaryItems()
    {
        var ret = new DictionaryBuilder();
        ret.WithItem(KnownNames.FunctionType, 2);
        ret.WithItem(KnownNames.Domain, DomainArray());
        ret.WithItem(KnownNames.N, exponent);
        if (!RangeIsDefault()) 
            ret.WithItem(KnownNames.Range, mappings.Select(i=>i.Range).AsPdfArray(mappings.Count));
        if (!TrivialC0())
            ret.WithItem(KnownNames.C0, new PdfArray(mappings.Select(i => (PdfIndirectObject)i.Bounds.MinValue).ToArray()));
        if (!TrivialC1())
            ret.WithItem(KnownNames.C1, new PdfArray(mappings.Select(i=>(PdfIndirectObject)i.Bounds.MaxValue).ToArray()));
        return ret;
    }

    private bool TrivialC0() => mappings.Count == 1 && mappings[0].Bounds.MinValue == 0;
    private bool TrivialC1() => mappings.Count == 1 && mappings[0].Bounds.MaxValue == 1;

    private PdfArray DomainArray() => 
        new(domain.MinValue, domain.MaxValue);

    private bool RangeIsDefault() => 
        mappings.All(i =>i.Range.Equals(ClosedInterval.NoRestriction));

    /// <summary>
    /// Add an output channel with the given maximum and minimum but no clipping.
    /// </summary>
    /// <param name="min">The output value when x = 0</param>
    /// <param name="max">The output value when x = 1</param>
    public void AddFunction(double min, double max) =>
        AddFunction(min, max, (double.MinValue, double.MaxValue));
    /// <summary>
    /// Add an output channel with the given maximum and minimum with output clipping.
    /// </summary>
    /// <param name="min">The output value when x = 0</param>
    /// <param name="max">The output value when x = 1</param>
    /// <param name="range">A range to which the output should be clipped.</param>
    public void AddFunction(double min, double max,  ClosedInterval range) => 
        mappings.Add(new ExponentialInterval((min, max), range));
}