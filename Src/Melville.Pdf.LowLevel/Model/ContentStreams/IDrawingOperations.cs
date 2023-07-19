using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// Content stream operators that define geometry or set clipping regions.
/// notably, this interface does not paint paths.
/// </summary>
public interface IPathDrawingOperations
{
    /// <summary>
    /// Content stream operator x y m
    /// </summary>
    void MoveTo(double x, double y);

    /// <summary>
    /// Content stream operator x y l
    /// </summary>
    void LineTo(double x, double y);

    /// <summary>
    /// Content stream operator control1X control1Y control2X control2Y finalX finalY c
    /// </summary>
    void CurveTo(
        double control1X, double control1Y,
        double control2X, double control2Y,
        double finalX, double finalY);

    /// <summary>
    /// Content stream operator control2X control2Y finalX finalY v
    /// </summary>
    void CurveToWithoutInitialControl(
        double control2X, double control2Y,
        double finalX, double finalY);

    /// <summary>
    /// Content stream operator control2X control2Y finalX finalY y
    /// </summary>
    void CurveToWithoutFinalControl(
        double control1X, double control1Y,
        double finalX, double finalY);

    /// <summary>
    /// Content stream operator h
    /// </summary>
    void ClosePath();

    /// <summary>
    /// Content stream operator x y width height re
    /// </summary>
    void Rectangle(double x, double y, double width, double height);
    
    /// <summary>
    /// Content stream operator S </summary>
    void StrokePath();
    
    /// <summary>
    /// Content stream operator s
    /// </summary>
    void CloseAndStrokePath();
    
    /// <summary>
    /// Content stream operator f
    /// </summary>
    void FillPath();
    
    /// <summary>
    /// Content stream operator f*
    /// </summary>
    void FillPathEvenOdd();
    
    /// <summary>
    /// Content stream operator B
    /// </summary>
    void FillAndStrokePath();
    
    /// <summary>
    /// Content stream operator B*
    /// </summary>
    void FillAndStrokePathEvenOdd();
    
    /// <summary>
    /// Content stream operator b
    /// </summary>
    void CloseFillAndStrokePath();
    
    /// <summary>
    /// Content stream operator b*
    /// </summary>
    void CloseFillAndStrokePathEvenOdd();

    /// <summary>
    /// Content stream operator n
    /// </summary>
    void EndPathWithNoOp();

    /// <summary>
    /// Content stream operator W
    /// </summary>
    void ClipToPath();

    /// <summary>
    /// Content stream operator W*
    /// </summary>
    void ClipToPathEvenOdd();
    
}

/// <summary>
/// An enhance IPathDrawingOperations that adds the ability to draw form XObjects
/// shaders, images, and inline images.
/// </summary>
public interface IDrawingOperations: IPathDrawingOperations
{
    /// <summary>
    /// Content stream operator name Do
    /// </summary>
    ValueTask DoAsync(PdfDirectValue name);
    /// <summary>
    /// Content stream operator name sh
    /// </summary>
    ValueTask PaintShaderAsync(PdfDirectValue name);
    /// <summary>
    /// Context stream operators BI, ID, and EI
    /// </summary>
    ValueTask DoAsync(PdfValueStream inlineImage);
}