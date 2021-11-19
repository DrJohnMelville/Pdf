using System;
using System.Numerics;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public enum TextRendering
{
    Fill = 0,
    Stroke = 1,
    FillAndStroke = 2,
    Invisible = 3,
    FillAndClip = 4,
    StrokeAndClip = 5
}

public interface IStateChangingOperations
{
    #region Non Text EXclusive operators

    /// <summary>
    /// Content Stream Operator q
    /// </summary>
    void SaveGraphicsState();
    
    /// <summary>
    /// Content Stream Operator Q
    /// </summary>
    void RestoreGraphicsState();

    /// <summary>
    /// Content Stream Operator a b c d e f cm
    /// </summary>
    void ModifyTransformMatrix(in Matrix3x2 newTransform);

    /// <summary>
    /// Content stream operator lineWidth w
    /// </summary>
    void SetLineWidth(double width);

    /// <summary>
    /// Content stream operator linecap J
    /// </summary>
    void SetLineCap(LineCap cap);

    /// <summary>
    /// Content stream operator lineJoinStyle j
    /// </summary>
    void SetLineJoinStyle(LineJoinStyle lineJoinStyle);

    /// <summary>
    /// Content stream operator miterLimit M
    /// </summary>
    void SetMiterLimit(double miter);

    /// <summary>
    /// Content stream operator dashArray dashphase
    /// Note the parameters are flipped from the PDF representation to accomodate a params extension method.
    /// </summary>
    void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray);

    /// <summary>
    /// Content stream operator renderingIntent ri
    /// </summary>
    /// <param name="intent"></param>
    void SetRenderIntent(RenderIntentName intent);

    /// <summary>
    /// Content stream operator tolerance i
    /// </summary>
    /// <param name="flatness"></param>
    void SetFlatnessTolerance(double flatness);

    /// <summary>
    /// Content stream operator gs
    /// </summary>
    void LoadGraphicStateDictionary(PdfName dictionaryName);

    #endregion

    #region TextAttribute
    /// <summary>
    /// Content stream operator charSpace Tc
    /// </summary>
    void SetCharSpace(double value);

    /// <summary>
    /// Content stream operator wordSpace Tw
    /// </summary>
    void SetWordSpace(double value);

    /// <summary>
    /// Content stream operator scale Tz
    /// </summary>
    void SetHorizontalTextScaling(double value);

    /// <summary>
    /// Content stream operator leading Tl
    /// </summary>
    void SetTextLeading(double value);

    /// <summary>
    /// Content stream operator font fontSize Tf
    /// </summary>
    void SetFont(PdfName font, double size);

    /// <summary>
    /// Context stream textRendering Tr
    /// </summary>
    /// <param name="rendering"></param>
    void SetTextRender(TextRendering rendering);

    /// <summary>
    /// Content stream operator rise Ts
    /// </summary>
    void SetTextRise(double value);

    #endregion
}

public static class StateChangingCSOperationsHelpers
{

    //This extension method is essentially a no-op it exists only to add a hint to the
    //intellisense that we might want to use a built in font name for this method
    public static void SetFont(
        this IStateChangingOperations target, BuiltInFontName fontName, double size) =>
        target.SetFont((PdfName)fontName, size);
    
    public static void SetLineDashPattern(
        this IStateChangingOperations target, double dashPhase = 0, params double[] dashArray) =>
        target.SetLineDashPattern(dashPhase, dashArray.AsSpan());
    
}