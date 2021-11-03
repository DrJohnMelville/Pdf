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

    public static PdfName SetFont(this IStateChangingCSOperations target, string fontName, double size)
    {
        var pdfName = NameDirectory.Get(fontName);
        target.SetFont(pdfName, size);
        return pdfName;
    }

    //This extension method is essentially a no-op it exists only to add a hint to the
    //intellisense that we might want to use a built in font name for this method
    public static void SetFont(
        this IStateChangingCSOperations target, BuiltInFontName fontName, double size) =>
        target.SetFont((PdfName)fontName, size);

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

    public static PdfName Do(this IDrawingCSOperations target, string name)
    {
        var pdfName = NameDirectory.Get(name);
        target.Do(pdfName);
        return pdfName;
    }

}

public interface IContentStreamOperations: 
    IStateChangingCSOperations, IDrawingCSOperations, IColorCSOperations
{
}