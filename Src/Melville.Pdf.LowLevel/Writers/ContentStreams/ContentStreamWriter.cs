using System;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;


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

    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
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

    #region Drawing Operations

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

    #endregion

    #region Color Operations

    public void SetStrokingColorSpace(PdfName colorSpace) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.CS, colorSpace);

    public void SetNonstrokingColorSpace(PdfName colorSpace) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.cs, colorSpace);

    #endregion

    public void SetStrokeColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.SC, components);

    public void SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName is not null) destPipe.WriteName(patternName);
        destPipe.WriteOperator(ContentStreamOperatorNames.SCN);
    }

    public void SetNonstrokingColor(in ReadOnlySpan<double> components) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.sc, components);

    public void SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        destPipe.WriteDoubleSpan(colors);
        if (patternName is not null) destPipe.WriteName(patternName);
        destPipe.WriteOperator(ContentStreamOperatorNames.scn);
    }

    public void SetStrokeGray(double grayLevel) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.G, grayLevel);

    public void SetStrokeRGB(double red, double green, double blue) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.RG, red, green, blue);

    public void SetStrokeCMYK(double cyan, double magenta, double yellow, double black) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.K, cyan, magenta, yellow, black);
    
    public void SetNonstrokingGray(double grayLevel) =>
        destPipe.WriteOperator(ContentStreamOperatorNames.g, grayLevel);

    public void SetNonstrokingRGB(double red, double green, double blue) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.rg, red, green, blue);

    public void SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black) => 
        destPipe.WriteOperator(ContentStreamOperatorNames.k, cyan, magenta, yellow, black);

}