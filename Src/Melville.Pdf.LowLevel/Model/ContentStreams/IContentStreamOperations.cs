using System;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IStateChangingCSOperations
{
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
    void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f);

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
    void SetLineJoinStyle(LineJoinStyle cap);

    /// <summary>
    /// Content stream operator miterLimit M
    /// </summary>
    void SetMiterLimit(double miter);

    /// <summary>
    /// Content stream operator dashArray dashphase
    /// Note the parameters are flipped from the PDF representation to accomodate a params extension method.
    /// </summary>
    void SetLineDashPattern(double dashPhase, ReadOnlySpan<double> dashArray);

    /// <summary>
    /// Content stream operator renderingIntent ri
    /// </summary>
    /// <param name="intent"></param>
    void SetRenderIntent(RenderingIntentName intent);
}

public static class ContentStreamExtendedOperations
{
    public static void SetLineDashPattern(
        this IStateChangingCSOperations target, double dashPhase = 0, params double[] dashArray) =>
        target.SetLineDashPattern(dashPhase, dashArray.AsSpan());
}

public interface IStatePreservingCSOperations
{
}

public interface IContentStreamOperations: IStateChangingCSOperations, IStatePreservingCSOperations
{
    
}