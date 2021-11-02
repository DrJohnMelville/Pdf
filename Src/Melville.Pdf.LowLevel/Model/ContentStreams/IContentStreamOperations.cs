using System;
using System.Runtime.InteropServices;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public static class ContentStreamExtendedOperations
{
    public static void SetLineDashPattern(
        this IStateChangingCSOperations target, double dashPhase = 0, params double[] dashArray) =>
        target.SetLineDashPattern(dashPhase, dashArray.AsSpan());
 
    public static PdfName LoadGraphicStateDictionary(
        this IStateChangingCSOperations target, string dictName)
    {
        var name = NameDirectory.Get(dictName);
        target.LoadGraphicStateDictionary(name);
        return name;
    }


    public static void SetStandardStrokingColorSpace(
        this IColorCSOperations target, ColorSpaceName colorSpace) =>
        target.SetStrokingColorSpace(colorSpace);
    public static void SetStandardNonstrokingColorSpace(
        this IColorCSOperations target, ColorSpaceName colorSpace) =>
        target.SetNonstrokingColorSpace(colorSpace);
    
    public static PdfName SetStrokingColorSpace(this IColorCSOperations target, string colorSpace)
    {
        var name = NameDirectory.Get(colorSpace);
        target.SetStrokingColorSpace(name);
        return name;
    }
    public static PdfName SetNonstrokingColorSpace(this IColorCSOperations target, string colorSpace)
    {
        var name = NameDirectory.Get(colorSpace);
        target.SetNonstrokingColorSpace(name);
        return name;
    }

    public static void SetStrokeColor(this IColorCSOperations target, params double[] colors) =>
        target.SetStrokeColor(new ReadOnlySpan<double>(colors));
    public static void SetStrokeColorExtended(this IColorCSOperations target, params double[] colors) =>
        target.SetStrokeColorExtended(null, new ReadOnlySpan<double>(colors));
    public static PdfName SetStrokeColorExtended(
        this IColorCSOperations target, string name, params double[] colors)
    {
        var pdfName = NameDirectory.Get(name);
        target.SetStrokeColorExtended(pdfName, new ReadOnlySpan<double>(colors));
        return pdfName;
    }
    
    public static void SetNonstrokingColor(this IColorCSOperations target, params double[] colors) =>
        target.SetNonstrokingColor(new ReadOnlySpan<double>(colors));
    public static void SetNonstrokingColorExtended(this IColorCSOperations target, params double[] colors) =>
        target.SetNonstrokingColorExtended(null, new ReadOnlySpan<double>(colors));
    public static PdfName SetNonstrokingColorExtended(
        this IColorCSOperations target, string name, params double[] colors)
    {
        var pdfName = NameDirectory.Get(name);
        target.SetNonstrokingColorExtended(pdfName, new ReadOnlySpan<double>(colors));
        return pdfName;
    }

}

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

public interface IContentStreamOperations: 
    IStateChangingCSOperations, IDrawingCSOperations, IColorCSOperations
{
}