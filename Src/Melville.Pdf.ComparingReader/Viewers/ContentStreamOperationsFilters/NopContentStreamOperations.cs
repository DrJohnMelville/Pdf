using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.ComparingReader.Viewers.ContentStreamOperationsFilters;

[StaticSingleton]
public partial class NopContentStreamOperations: IContentStreamOperations
{
    /// <inheritdoc />
    public void SaveGraphicsState()
    {
    }

    /// <inheritdoc />
    public void RestoreGraphicsState()
    {
    }

    /// <inheritdoc />
    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
    }

    /// <inheritdoc />
    public void SetLineWidth(double width)
    {
    }

    /// <inheritdoc />
    public void SetLineCap(LineCap cap)
    {
    }

    /// <inheritdoc />
    public void SetLineJoinStyle(LineJoinStyle lineJoinStyle)
    {
    }

    /// <inheritdoc />
    public void SetMiterLimit(double miter)
    {
    }

    /// <inheritdoc />
    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
    }

    /// <inheritdoc />
    public void SetFlatnessTolerance(double flatness)
    {
    }

    /// <inheritdoc />
    public void SetCharSpace(double value)
    {
    }

    /// <inheritdoc />
    public void SetWordSpace(double value)
    {
    }

    /// <inheritdoc />
    public void SetHorizontalTextScaling(double value)
    {
    }

    /// <inheritdoc />
    public void SetTextLeading(double value)
    {
    }

    /// <inheritdoc />
    public ValueTask SetFontAsync(PdfDirectObject font, double size) => default;

    /// <inheritdoc />
    public void SetTextRender(TextRendering rendering)
    {
    }

    /// <inheritdoc />
    public void SetTextRise(double value)
    {
    }

    /// <inheritdoc />
    public void MoveTo(double x, double y)
    {
    }

    /// <inheritdoc />
    public void LineTo(double x, double y)
    {
    }

    /// <inheritdoc />
    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY)
    {
    }

    /// <inheritdoc />
    public void CurveToWithoutInitialControl(double control2X, double control2Y, double finalX, double finalY)
    {
    }

    /// <inheritdoc />
    public void CurveToWithoutFinalControl(double control1X, double control1Y, double finalX, double finalY)
    {
    }

    /// <inheritdoc />
    public void ClosePath()
    {
    }

    /// <inheritdoc />
    public void Rectangle(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc />
    public void StrokePath()
    {
    }

    /// <inheritdoc />
    public void CloseAndStrokePath()
    {
    }

    /// <inheritdoc />
    public void FillPath()
    {
    }

    /// <inheritdoc />
    public void FillPathEvenOdd()
    {
    }

    /// <inheritdoc />
    public void FillAndStrokePath()
    {
    }

    /// <inheritdoc />
    public void FillAndStrokePathEvenOdd()
    {
    }

    /// <inheritdoc />
    public void CloseFillAndStrokePath()
    {
    }

    /// <inheritdoc />
    public void CloseFillAndStrokePathEvenOdd()
    {
    }

    /// <inheritdoc />
    public void EndPathWithNoOp()
    {
    }

    /// <inheritdoc />
    public void ClipToPath()
    {
    }

    /// <inheritdoc />
    public void ClipToPathEvenOdd()
    {
    }

    /// <inheritdoc />
    public ValueTask DoAsync(PdfDirectObject name) => default;

    /// <inheritdoc />
    public ValueTask PaintShaderAsync(PdfDirectObject name) => default;

    /// <inheritdoc />
    public ValueTask DoAsync(PdfStream inlineImage) => default;

    /// <inheritdoc />
    public ValueTask SetStrokingColorSpaceAsync(PdfDirectObject colorSpace) => default;

    /// <inheritdoc />
    public ValueTask SetNonstrokingColorSpaceAsync(PdfDirectObject colorSpace) => default;

    /// <inheritdoc />
    public void SetStrokeColor(in ReadOnlySpan<double> components)
    {
    }

    /// <inheritdoc />
    public void SetNonstrokingColor(in ReadOnlySpan<double> components)
    {
    }

    /// <inheritdoc />
    public ValueTask SetStrokeColorExtendedAsync(PdfDirectObject? patternName, in ReadOnlySpan<double> colors) => default;

    /// <inheritdoc />
    public ValueTask SetNonstrokingColorExtendedAsync(PdfDirectObject? patternName, in ReadOnlySpan<double> colors) => default;

    /// <inheritdoc />
    public ValueTask SetStrokeGrayAsync(double grayLevel) => default;

    /// <inheritdoc />
    public ValueTask SetStrokeRGBAsync(double red, double green, double blue) => default;

    /// <inheritdoc />
    public ValueTask SetStrokeCMYKAsync(double cyan, double magenta, double yellow, double black) => default;

    /// <inheritdoc />
    public ValueTask SetNonstrokingGrayAsync(double grayLevel) => default;

    /// <inheritdoc />
    public ValueTask SetNonstrokingRgbAsync(double red, double green, double blue) => default;

    /// <inheritdoc />
    public ValueTask SetNonstrokingCMYKAsync(double cyan, double magenta, double yellow, double black) => default;

    /// <inheritdoc />
    public void SetRenderIntent(RenderIntentName intent)
    {
    }

    /// <inheritdoc />
    public void MovePositionBy(double x, double y)
    {
    }

    /// <inheritdoc />
    public void MovePositionByWithLeading(double x, double y)
    {
    }

    /// <inheritdoc />
    public void SetTextMatrix(double a, double b, double c, double d, double e, double f)
    {
    }

    /// <inheritdoc />
    public void MoveToNextTextLine()
    {
    }

    /// <inheritdoc />
    public ValueTask ShowStringAsync(ReadOnlyMemory<byte> decodedString) => default;

    /// <inheritdoc />
    public ValueTask MoveToNextLineAndShowStringAsync(ReadOnlyMemory<byte> decodedString) => default;

    /// <inheritdoc />
    public ValueTask MoveToNextLineAndShowStringAsync(double wordSpace, double charSpace, ReadOnlyMemory<byte> decodedString) => default;

    /// <inheritdoc />
    public ISpacedStringBuilder GetSpacedStringBuilder() => 
        new NopSpacedStringBuilder();


    /// <inheritdoc />
    public void BeginTextObject()
    {
    }

    /// <inheritdoc />
    public void EndTextObject()
    {
    }

    /// <inheritdoc />
    public void MarkedContentPoint(PdfDirectObject tag)
    {
    }

    /// <inheritdoc />
    public ValueTask MarkedContentPointAsync(PdfDirectObject tag, PdfDirectObject properties)
    {
        return default;
    }

    /// <inheritdoc />
    public ValueTask MarkedContentPointAsync(PdfDirectObject tag, PdfDictionary dictionary) => default;

    /// <inheritdoc />
    public void BeginMarkedRange(PdfDirectObject tag)
    {
    }

    /// <inheritdoc />
    public ValueTask BeginMarkedRangeAsync(PdfDirectObject tag, PdfDirectObject dictName) => default;

    /// <inheritdoc />
    public ValueTask BeginMarkedRangeAsync(PdfDirectObject tag, PdfDictionary dictionary) => default;

    /// <inheritdoc />
    public void EndMarkedRange()
    {
    }

    /// <inheritdoc />
    public void BeginCompatibilitySection()
    {
    }

    /// <inheritdoc />
    public void EndCompatibilitySection()
    {
    }

    /// <inheritdoc />
    public void SetColoredGlyphMetrics(double wX, double wY)
    {
    }

    /// <inheritdoc />
    public void SetUncoloredGlyphMetrics(double wX, double wY, double llX, double llY, double urX, double urY)
    {
    }

    /// <inheritdoc />
    public ValueTask LoadGraphicStateDictionaryAsync(PdfDirectObject dictionaryName) => default;


}