using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// This class specifies extension medhods for the IColorOperations interface.  This class
/// is largely convenience methods for the PDF generators.
/// </summary>
public static class ColorOperationsHelpers
{
    // the next two methods are no-ops than hint the intellisense to the specific
    // subclass of pdfName

    /// <summary>
    /// Set the stroking color space in the receiver
    /// </summary>
    /// <param name="target">Receiver for the extension method</param>
    /// <param name="colorSpace">The desired colorspace.</param>
    /// <returns>A completed valuetask</returns>
    public static ValueTask SetStrokingColorSpaceAsync(
        this IColorOperations target, ColorSpaceName colorSpace) =>
        target.SetStrokingColorSpaceAsync(colorSpace);
    /// <summary>
    /// Set the Nonstroking color space in the receiver
    /// </summary>
    /// <param name="target">Receiver for the extension method</param>
    /// <param name="colorSpace">The desired colorspace.</param>
    /// <returns>A completed valuetask</returns>
    public static ValueTask SetNonstrokingColorSpaceAsync(
        this IColorOperations target, ColorSpaceName colorSpace) =>
        target.SetNonstrokingColorSpaceAsync(colorSpace);

    /// <summary>
    /// Sets the stroking color in the receiver.
    /// </summary>
    /// <param name="target">Receiver whose color is to be set</param>
    /// <param name="colors">The color desired, in the current colorspace</param>
    public static void SetStrokeColor(this IColorOperations target, params double[] colors) =>
        target.SetStrokeColor(new ReadOnlySpan<double>(colors));
    /// <summary>
    /// Sets the stroking color in the receiver, using the extended syntax with a null name.
    /// </summary>
    /// <param name="target">Receiver whose color is to be set</param>
    /// <param name="colors">The color desired, in the current colorspace</param>
    /// <returns>A valuetask representing completion of this task</returns>
    public static ValueTask SetStrokeColorExtendedAsync(this IColorOperations target, params double[] colors) =>
        target.SetStrokeColorExtendedAsync(null, new ReadOnlySpan<double>(colors));
    /// <summary>
    /// Sets the stroking color in the receiver with a name and color values.
    /// </summary>
    /// <param name="target">Receiver whose color is to be set</param>
    /// <param name="name">A PDF name, typically of a pattern, to set as the present color</param> 
    /// <param name="colors">The numeric color values, if any</param>
    /// <returns>A ValueTask representing completion of this operation</returns>
    public static ValueTask SetStrokeColorExtendedAsync(
        this IColorOperations target, PdfDirectValue? name, params double[] colors) =>
        target.SetStrokeColorExtendedAsync(name, new ReadOnlySpan<double>(colors));

    /// <summary>
    /// Sets the nonstroking color in the receiver.
    /// </summary>
    /// <param name="target">Receiver who'se color is to be set</param>
    /// <param name="colors">The color desired, in the current colorspace</param>
    public static void SetNonstrokingColor(this IColorOperations target, params double[] colors) =>
        target.SetNonstrokingColor(new ReadOnlySpan<double>(colors));
    /// <summary>
    /// Sets the nonstroking color in the receiver, using the extended syntax with a null name.
    /// </summary>
    /// <param name="target">Receiver who'se color is to be set</param>
    /// <param name="colors">The color desired, in the current colorspace</param>
    /// <returns>A valuetask representing completion of this task</returns>
    public static ValueTask SetNonstrokingColorExtendedAsync(this IColorOperations target, params double[] colors) =>
        target.SetNonstrokingColorExtendedAsync(null, new ReadOnlySpan<double>(colors));
    /// <summary>
    /// Sets the stroking color in the receiver with a name and color values.
    /// </summary>
    /// <param name="target">Receiver whose color is to be set</param>
    /// <param name="name">A PDF name, typically of a pattern, to set as the present color</param> 
    /// <param name="colors">The numeric color values, if any</param>
    /// <returns>A ValueTask representing completion of this operation</returns>
    public static ValueTask SetNonstrokingColorExtendedAsync(
        this IColorOperations target, PdfDirectValue? name, params double[] colors) =>
        target.SetNonstrokingColorExtendedAsync(name, new ReadOnlySpan<double>(colors));
}