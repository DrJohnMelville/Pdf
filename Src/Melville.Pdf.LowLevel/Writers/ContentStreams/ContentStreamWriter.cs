using System;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

[MacroItem("q", "SaveGraphicsState")]
[MacroItem("Q", "RestoreGraphicsState")]
[MacroItem("h", "ClosePath")]
[MacroItem("S", "StrokePath")]
[MacroItem("s", "CloseAndStrokePath")]
[MacroItem("f", "FillPath")]
[MacroItem("fStar", "FillPathEvenOdd")]
[MacroItem("B", "FillAndStrokePath")]
[MacroItem("BStar", "FillAndStrokePathEvenOdd")]
[MacroItem("b", "CloseFillAndStrokePath")]
[MacroItem("bStar", "CloseFillAndStrokePathEvenOdd")]
[MacroItem("n","EndPathWithNoOp")]
[MacroItem("W","ClipToPath")]
[MacroItem("WStar","ClipToPathEvenOdd")]
[MacroCode("public void ~1~() => destPipe.WriteOperator(ContentStreamOperatorNames.~0~);")]

public partial class ContentStreamWriter : IContentStreamOperations
{
    private readonly ContentStreamPipeWriter destPipe;

    public ContentStreamWriter(PipeWriter destPipe)
    {
        this.destPipe = new ContentStreamPipeWriter(destPipe);
    }

    #region Graphic State Operations
    public void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.cm, a, b, c, d, e, f);

    public void SetLineWidth(double width) => destPipe.WriteOperator(ContentStreamOperatorNames.w, width);

    public void SetLineCap(LineCap cap) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.J, (double)cap);

    public void SetLineJoinStyle(LineJoinStyle cap) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.j, (double)cap);

    public void SetMiterLimit(double miter) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.M, miter);

    public void SetLineDashPattern(double dashPhase, ReadOnlySpan<double> dashArray)
    {
        destPipe.WriteDoubleArray(dashArray);
        destPipe.WriteOperator(ContentStreamOperatorNames.d, dashPhase);
    }

    public void SetRenderIntent(RenderingIntentName intent) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.ri, intent);

    public void SetFlatnessTolerance(double flatness) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.i, flatness);

    public void LoadGraphicStateDictionary(PdfName dictionaryName) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.gs, dictionaryName);

    #endregion

    public void MoveTo(double x, double y) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.m, x, y);

    public void LineTo(double x, double y) => destPipe.WriteOperator(ContentStreamOperatorNames.l, x,y);

    public void CurveTo(
        double control1X, double control1Y,
        double control2X, double control2Y,
        double finalX, double finalY) => destPipe.WriteOperator(ContentStreamOperatorNames.c, 
            control1X, control1Y, control2X, control2Y, finalX, finalY);

    public void CurveToWithoutInitialControl(
       double control2X, double control2Y,
        double finalX, double finalY)=> destPipe.WriteOperator(ContentStreamOperatorNames.v, 
         control2X, control2Y, finalX, finalY);

    public void CurveToWithoutFinalControl(
       double control1X, double control1Y,
        double finalX, double finalY)=> destPipe.WriteOperator(ContentStreamOperatorNames.y, 
         control1X, control1Y, finalX, finalY);

    public void Rectangle(double x, double y, double width, double height) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.re, x, y, width, height);
}