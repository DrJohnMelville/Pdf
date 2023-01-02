namespace Melville.Pdf.LowLevel.Writers.Builder.Functions;

/// <summary>
/// Enumeration describing the kinds of interpolation supported by sampled functions
/// </summary>
public enum SampledFunctionOrder
{
    /// <summary>
    /// Linear interpolation
    /// </summary>
    Linear = 1,
    /// <summary>
    /// Bicubic interpolation.
    /// </summary>
    Cubic = 3
}