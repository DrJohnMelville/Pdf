using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions;

public class S7_10_1GeneralFunctions
{
    private class GenericFunction : PdfFunction
    {
        public GenericFunction(ClosedInterval[] domain, ClosedInterval[] range) : base(domain, range)
        {
        }

        protected override void ComputeOverride(in ReadOnlySpan<double> input, in Span<double> result)
        {
            input.CopyTo(result);
        }
    }

    [Fact]
    public void EvaluateSimpleFunc()
    {
        var values = new double[] {1,2,3};
        Assert.Equal(values, new GenericFunction(
            new[] { ClosedInterval.NoRestriction, ClosedInterval.NoRestriction, ClosedInterval.NoRestriction },
            new[] { ClosedInterval.NoRestriction, ClosedInterval.NoRestriction, ClosedInterval.NoRestriction }
        ).Compute(values));
    }


    public static IEnumerable<object[]> RangeTests()
    {
        foreach (var x in new double []{-5,-1,0,1,4,9,10,11,17})
        foreach (var y in new double[]{-5,-1,0,25,99,100,101,1000})
        {
            yield return new object[]{ x, y, x.Clamp(0, 10), y.Clamp(0, 100) };
        }
    }
    [Theory]
    [MemberData(nameof(RangeTests))]
    public void DomainClippingTest(double x, double y, double xout, double yout)
    {
        ReadOnlySpan<double> values = stackalloc double[] { x, y };
        double[] output = new[] { xout, yout };
        Assert.Equal(output, new GenericFunction(
            new []{new ClosedInterval(0, 10), new ClosedInterval(0, 100) },
            new []{ClosedInterval.NoRestriction, ClosedInterval.NoRestriction }
        ).Compute(values).ToArray());
    }
    [Theory]
    [MemberData(nameof(RangeTests))]
    public void RangeClippingTest(double x, double y, double xout, double yout)
    {
        ReadOnlySpan<double> values = stackalloc double[] { x, y };
        double[] output = new[] { xout, yout };
        Assert.Equal(output, new GenericFunction(
            new []{ClosedInterval.NoRestriction, ClosedInterval.NoRestriction },
            new []{new ClosedInterval(0, 10), new ClosedInterval(0, 100) }
        ).Compute(values).ToArray());
    }

    [Fact]
    public void GetSingleValue()
    {
        var func = new GenericFunction(
            new[] { new ClosedInterval(0, 10), new ClosedInterval(0, 100) },
            new[] { ClosedInterval.NoRestriction, ClosedInterval.NoRestriction }
        );
        Assert.Equal(5, func.ComputeSingleResult(5));
        Assert.Equal(0, func.ComputeSingleResult(5,1));
        Assert.Equal(2, func.ComputeSingleResult(new double[]{5,2},1));
        Assert.Equal(2, func.Compute(new double[]{5,2})[1]);
        Assert.Equal(5, func.Compute(5)[0]);
    }
}