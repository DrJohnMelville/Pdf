using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// PDF spec 2.0 clause 8.6.8 dictates color operators which are turned off at certian times
/// all the operators in this interface get turned off at the right times.
/// </summary>
public interface IColorOperations
{
    /// <summary>
    /// Content stream operator CS
    /// </summary>
    ValueTask SetStrokingColorSpaceAsync(PdfDirectObject colorSpace);

    /// <summary>
    /// Content stream operator cs
    /// </summary>
    ValueTask SetNonstrokingColorSpaceAsync(PdfDirectObject colorSpace);

    /// <summary>
    /// Content stream operator SC
    /// </summary>
    void SetStrokeColor(in ReadOnlySpan<double> components);

    /// <summary>
    /// Content stream operator sc
    /// </summary>
    void SetNonstrokingColor(in ReadOnlySpan<double> components);
    
    /// <summary>
    /// Content stream operator SCN
    /// </summary>
    ValueTask SetStrokeColorExtendedAsync(PdfDirectObject? patternName, in ReadOnlySpan<double> colors);
    
    /// <summary>
    /// Content stream operator scn
    /// </summary>
    ValueTask SetNonstrokingColorExtendedAsync(PdfDirectObject? patternName, in ReadOnlySpan<double> colors);
    
    /// <summary>
    /// Content stream operator G
    /// </summary>
    ValueTask SetStrokeGrayAsync(double grayLevel);

    /// <summary>
    /// Content stream operator RG
    /// </summary>
    ValueTask SetStrokeRGBAsync(double red, double green, double blue);

    /// <summary>
    /// Content stream operator K
    /// </summary>
    ValueTask SetStrokeCMYKAsync(double cyan, double magenta, double yellow, double black);
    
    /// <summary>
    /// Content stream operator g
    /// </summary>
    ValueTask SetNonstrokingGrayAsync(double grayLevel);

    /// <summary>
    /// Content stream operator rg
    /// </summary>
    ValueTask SetNonstrokingRgbAsync(double red, double green, double blue);

    /// <summary>
    /// Content stream operator k
    /// </summary>
    ValueTask SetNonstrokingCMYKAsync(double cyan, double magenta, double yellow, double black);
    
    /// <summary>
    /// Content stream operator renderingIntent ri
    /// </summary>
    void SetRenderIntent(RenderIntentName intent);

}