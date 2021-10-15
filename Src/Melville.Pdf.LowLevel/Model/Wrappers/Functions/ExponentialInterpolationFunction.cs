using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions
{
    public sealed class ExponentialInterpolationFunction: PdfFunction
    {
        private readonly ClosedInterval[] transforms;
        private readonly double exponent;
        public ExponentialInterpolationFunction(
            ClosedInterval[] domain, ClosedInterval[] range,
            ClosedInterval[] transforms, double exponent) : base(domain, range)
        {
            this.transforms = transforms;
            this.exponent = exponent;
        }

        protected override void ComputeOverride(in ReadOnlySpan<double> input, in Span<double> result)
        {
            Debug.Assert(input.Length == 1);
            var source = new ClosedInterval(0, 1);
            var exponentiatedInput = Math.Pow(input[0], exponent);
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = source.MapTo(transforms[i], exponentiatedInput);
            }
        }
    }
}