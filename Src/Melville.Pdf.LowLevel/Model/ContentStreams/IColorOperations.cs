using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IColorOperations
{
    /// <summary>
    /// Content stream operator CS
    /// </summary>
    ValueTask SetStrokingColorSpace(PdfName colorSpace);

    /// <summary>
    /// Content stream operator cs
    /// </summary>
    ValueTask SetNonstrokingColorSpace(PdfName colorSpace);

    
    /// <summary>
    /// Content stream operator SCN
    /// </summary>
    ValueTask SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors);
    
    /// <summary>
    /// Content stream operator scn
    /// </summary>
    ValueTask SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors);
    
    /// <summary>
    /// Content stream operator G
    /// </summary>
    ValueTask SetStrokeGray(double grayLevel);

    /// <summary>
    /// Content stream operator RG
    /// </summary>
    ValueTask SetStrokeRGB(double red, double green, double blue);

    /// <summary>
    /// Content stream operator K
    /// </summary>
    ValueTask SetStrokeCMYK(double cyan, double magenta, double yellow, double black);
    
    /// <summary>
    /// Content stream operator g
    /// </summary>
    ValueTask SetNonstrokingGray(double grayLevel);

    /// <summary>
    /// Content stream operator rg
    /// </summary>
    ValueTask SetNonstrokingRGB(double red, double green, double blue);

    /// <summary>
    /// Content stream operator k
    /// </summary>
    ValueTask SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black);
    
    /// <summary>
    /// Content stream operator SC
    /// </summary>
    void SetStrokeColor(in ReadOnlySpan<double> components);

    /// <summary>
    /// Content stream operator sc
    /// </summary>
    void SetNonstrokingColor(in ReadOnlySpan<double> components);


}

public static class ColorCSOperationsHelpers
{
    // the next two methods are no-ops than hint the intellisense to the specific
    // subclass of pdfName
    public static ValueTask SetStrokingColorSpace(
        this IColorOperations target, ColorSpaceName colorSpace) =>
        target.SetStrokingColorSpace(colorSpace);
    public static ValueTask SetNonstrokingColorSpace(
        this IColorOperations target, ColorSpaceName colorSpace) =>
        target.SetNonstrokingColorSpace(colorSpace);

    public static void SetStrokeColor(this IColorOperations target, params double[] colors) =>
        target.SetStrokeColor(new ReadOnlySpan<double>(colors));
    public static ValueTask SetStrokeColorExtended(this IColorOperations target, params double[] colors) =>
        target.SetStrokeColorExtended(null, new ReadOnlySpan<double>(colors));
    public static ValueTask SetStrokeColorExtended(
        this IColorOperations target, PdfName? name, params double[] colors) =>
        target.SetStrokeColorExtended(name, new ReadOnlySpan<double>(colors));

    public static void SetNonstrokingColor(this IColorOperations target, params double[] colors) =>
        target.SetNonstrokingColor(new ReadOnlySpan<double>(colors));
    public static ValueTask SetNonstrokingColorExtended(this IColorOperations target, params double[] colors) =>
        target.SetNonstrokingColorExtended(null, new ReadOnlySpan<double>(colors));
    public static ValueTask SetNonstrokingColorExtended(
        this IColorOperations target, PdfName? name, params double[] colors) =>
        target.SetNonstrokingColorExtended(name, new ReadOnlySpan<double>(colors));
}