using System;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions;

public interface IPdfFunction
{
    ClosedInterval[] Domain { get; }
    ClosedInterval[] Range { get; }
    void Compute(in ReadOnlySpan<double> input, in Span<double> result);
}

public static class PdfFunctionHelpers
{
    public static double[] Compute(this IPdfFunction func, in ReadOnlySpan<double> input)
    {
        var result = new double[func.Range.Length];
        func.Compute(input, result);
        return result;
    }
    public static double[] Compute(this IPdfFunction func, double i) => 
        func.Compute(InputSpan(i, stackalloc double[func.Domain.Length]));
    public static void Compute(this IPdfFunction func, double i, Span<double> result) => 
        func.Compute(InputSpan(i, stackalloc double[func.Domain.Length]), result);

    private static ReadOnlySpan<double> InputSpan(double d, in Span<double> span)
    {
        span[0] = d;
        return span;
    }

    public static double ComputeSingleResult(this IPdfFunction func, double input, int desired = 0) =>
        func.ComputeSingleResult(InputSpan(input, stackalloc double[func.Domain.Length]), desired);
    
    public static double ComputeSingleResult(
        this IPdfFunction function, in ReadOnlySpan<double> input, int desired = 0)
    {
        Span<double> ret = stackalloc double[function.Range.Length];
        function.Compute(input, ret);
        return ret[desired];
    }

}