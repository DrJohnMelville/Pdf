using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public readonly struct ExponentialInterval
    {
        public ClosedInterval Bounds { get; }
        public ClosedInterval Range { get; }

        public ExponentialInterval(ClosedInterval bounds, ClosedInterval range)
        {
            Bounds = bounds;
            Range = range;
        }
    }
    public class ExponentialFunctionBuilder
    {
        private ClosedInterval domain;
        private double exponent;
        private List<ExponentialInterval> mappings = new();

        public ExponentialFunctionBuilder(double exponent, ClosedInterval? domain = null)
        {
            this.exponent = exponent;
            this.domain = domain ?? new ClosedInterval(0,1);
        }

        public PdfDictionary Create() => new(DictionaryItems());

        private IEnumerable<(PdfName, PdfObject)> DictionaryItems()
        {
            var ret = new List<(PdfName, PdfObject)>();
            ret.Add((KnownNames.FunctionType, new PdfInteger(2)));
            ret.Add((KnownNames.Domain, DomainArray()));
            ret.Add((KnownNames.N, new PdfDouble(exponent)));
            if (!RangeIsDefault()) 
                ret.Add((KnownNames.Range, mappings.Select(i=>i.Range).AsPdfArray(mappings.Count)));
            if (!TrivialC0())
                ret.Add((KnownNames.C0, new PdfArray(mappings.Select(i=>new PdfDouble(i.Bounds.MinValue)))));
            if (!TrivialC1())
                ret.Add((KnownNames.C1, new PdfArray(mappings.Select(i=>new PdfDouble(i.Bounds.MaxValue)))));
            return ret;
        }

        private bool TrivialC0() => mappings.Count == 1 && mappings[0].Bounds.MinValue == 0;
        private bool TrivialC1() => mappings.Count == 1 && mappings[0].Bounds.MaxValue == 1;

        private PdfArray DomainArray() => 
            new(new PdfDouble(domain.MinValue), new PdfDouble(domain.MaxValue));

        private bool RangeIsDefault() => 
            mappings.All(i =>i.Range.Equals(ClosedInterval.NoRestriction));

        public void AddFunction(double min, double max) =>
            AddFunction(min, max, (double.MinValue, double.MaxValue));
        public void AddFunction(double min, double max,  ClosedInterval range) => 
            mappings.Add(new ExponentialInterval((min, max), range));
    }
}