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
    ValueTask SetStrokingColorSpace(PdfName colorSpace);

    /// <summary>
    /// Content stream operator cs
    /// </summary>
    ValueTask SetNonstrokingColorSpace(PdfName colorSpace);

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
    /// Content stream operator renderingIntent ri
    /// </summary>
    void SetRenderIntent(RenderIntentName intent);

}