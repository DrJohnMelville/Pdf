using System;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions;

/// <summary>
/// Represents a PDF function
/// </summary>
public interface IPdfFunction
{
    /// <summary>
    /// The range of the domains of the functions.  PDF functions can have multiple inputs, with different domains.
    /// </summary>
    ClosedInterval[] Domain { get; }
    /// <summary>
    /// A range for each of the outputs of this function.
    /// </summary>
    ClosedInterval[] Range { get; }
    /// <summary>
    /// Compute a Pdf function for a given set of inputs
    /// </summary>
    /// <param name="input">A span of doubles representing the inputs to the function.  Must be at least as long as the Domain array.</param>    
    /// <param name="result">A span of doubles too receive the outputs of the function.  Must be at least as long as the range array.</param>
    void Compute(in ReadOnlySpan<double> input, in Span<double> result);
}

/// <summary>
/// The IPdfFunction interface has an API designed to allow consumers to minimize allocations
/// by utilizing output spans.  This class adds some sugar around common function call scenerios.
/// </summary>
public static class PdfFunctionHelpers
{
    /// <summary>
    /// Call a PdfFunction and allocate a new array on the heap to receive the result.
    /// </summary>
    /// <param name="func">The function to call.</param>
    /// <param name="input">A span of doubles representing the inputs to the function.  Must be at least as long as the Domain array.</param>    
    /// <returns>A heap allocated array containing the result of the function.</returns>
    public static double[] Compute(this IPdfFunction func, in ReadOnlySpan<double> input)
    {
        var result = new double[func.Range.Length];
        func.Compute(input, result);
        return result;
    }

    /// <summary>
    /// Call a PdfFunction with a single argument and allocate a new array on the heap to receive the result.
    /// </summary>
    /// <param name="func">The function to call.</param>
    /// <param name="i">Input to the function.</param>
    /// <returns>A heap allocated array containing the result of the function.</returns>
    public static double[] Compute(this IPdfFunction func, double i) => 
        func.Compute(InputSpan(i, stackalloc double[func.Domain.Length]));
    /// <summary>
    /// Call a PDF function with a single argument.
    /// </summary>
    /// <param name="func">A pdf function</param>
    /// <param name="i">Argument to the function</param>
    /// <param name="result">A span of doubles too receive the outputs of the function.  Must be at least as long as the range array.</param>
    public static void Compute(this IPdfFunction func, double i, Span<double> result) => 
        func.Compute(InputSpan(i, stackalloc double[func.Domain.Length]), result);

    private static ReadOnlySpan<double> InputSpan(double d, in Span<double> span)
    {
        span[0] = d;
        return span;
    }

    /// <summary>
    /// Call a PDF function with a single double input and return a single double result.
    /// </summary>
    /// <param name="func">The functions </param>
    /// <param name="input">A double input to the function.</param>    
    /// <param name="desired">An ordinal value for the desired output of the function to return.  Must be less than the length of Ranges.</param>
    /// <returns>The desired value from the result of evaluating the function on the input.</returns>
    public static double ComputeSingleResult(this IPdfFunction func, double input, int desired = 0) =>
        func.ComputeSingleResult(InputSpan(input, stackalloc double[func.Domain.Length]), desired);

    /// <summary>
    /// Call a PDF function and return a single double result.
    /// </summary>
    /// <param name="function">The functions </param>
    /// <param name="input">A span of doubles representing the inputs to the function.  Must be at least as long as the Domain array.</param>    
    /// <param name="desired">An ordinal value for the desired output of the function to return.  Must be less than the length of Ranges.</param>
    /// <returns>The desired value from the result of evaluating the function on the input.</returns>
    public static double ComputeSingleResult(
        this IPdfFunction function, in ReadOnlySpan<double> input, int desired = 0)
    {
        Span<double> ret = stackalloc double[function.Range.Length];
        function.Compute(input, ret);
        return ret[desired];
    }

}