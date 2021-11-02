using System;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IColorCSOperations
{
    /// <summary>
    /// Content stream operator CS
    /// </summary>
    void SetStrokingColorSpace(PdfName colorSpace);

    /// <summary>
    /// Content stream operator cs
    /// </summary>
    void SetNonstrokingColorSpace(PdfName colorSpace);

    /// <summary>
    /// Content stream operator SC
    /// </summary>
    void SetStrokeColor(in ReadOnlySpan<double> components);
    
    /// <summary>
    /// Content stream operator SCN
    /// </summary>
    void SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors);

    /// <summary>
    /// Content stream operator sc
    /// </summary>
    void SetNonstrokingColor(in ReadOnlySpan<double> components);
    
    /// <summary>
    /// Content stream operator scn
    /// </summary>
    void SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors);
    
    /// <summary>
    /// Content stream operator G
    /// </summary>
    void SetStrokeGray(double grayLevel);

    /// <summary>
    /// Content stream operator RG
    /// </summary>
    void SetStrokeRGB(double red, double green, double blue);

    /// <summary>
    /// Content stream operator K
    /// </summary>
    void SetStrokeCMYK(double cyan, double magenta, double yellow, double black);
    
    /// <summary>
    /// Content stream operator g
    /// </summary>
    void SetNonstrokingGray(double grayLevel);

    /// <summary>
    /// Content stream operator rg
    /// </summary>
    void SetNonstrokingRGB(double red, double green, double blue);

    /// <summary>
    /// Content stream operator k
    /// </summary>
    void SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black);

}