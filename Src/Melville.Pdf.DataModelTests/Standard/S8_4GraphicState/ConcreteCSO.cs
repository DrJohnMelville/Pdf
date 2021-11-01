using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

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
[MacroCode("public virtual void ~1~(){}")]
public partial class ConcreteCSO: IContentStreamOperations
{
    public virtual void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f)
    {
    }

    public virtual void SetLineWidth(double width)
    {
    }

    public virtual void SetLineCap(LineCap cap)
    {
    }

    public virtual void SetLineJoinStyle(LineJoinStyle cap)
    {
    }

    public virtual void SetMiterLimit(double miter)
    {
    }

    public virtual void TestSetLineDashPattern(double dashPhase, double[] dashArray)
    {
    }

    public void SetLineDashPattern(
        double dashPhase, ReadOnlySpan<double> dashArray) =>
        TestSetLineDashPattern(dashPhase, dashArray.ToArray());

    public virtual void SetRenderIntent(RenderingIntentName intent)
    {
    }

    public virtual void SetFlatnessTolerance(double flatness)
    {
    }

    public virtual void LoadGraphicStateDictionary(PdfName dictionaryName)
    {
    }

    public virtual void MoveTo(double x, double y)
    {
    }

    public virtual void LineTo(double x, double y)
    {
    }

    public virtual void CurveTo(double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY)
    {
    }
    
    public virtual void CurveToWithoutInitialControl(
        double control2X, double control2Y,
        double finalX, double finalY) {}
    
    public virtual void CurveToWithoutFinalControl(
        double control1X, double control1Y,
        double finalX, double finalY) {}
    
    public virtual void Rectangle(double x, double y, double width, double height){}
    
    public virtual void SetStrokingColorSpace(PdfName colorSpace){}
    public virtual void SetNonstrokingColorSpace(PdfName colorSpace){}

    public virtual void SetStrokeColor(in ReadOnlySpan<double> components) =>
        TestSetStrokeColor(components.ToArray());
    public virtual void TestSetStrokeColor(double[] components) { }

    public virtual void SetStrokeColorExtended(PdfName? patternName, ReadOnlySpan<double> colors) =>
        TestSetStrokeColorExtended(patternName, colors.ToArray());

    public virtual void TestSetStrokeColorExtended(PdfName? pdfName, double[] toArray)
    {
    }
    public virtual void SetNonstrokingColor(in ReadOnlySpan<double> components) =>
        TestSetNonstrokingColor(components.ToArray());
    public virtual void TestSetNonstrokingColor(double[] components) { }

    public virtual void SetNonstrokingColorExtended(PdfName? patternName, ReadOnlySpan<double> colors) =>
        TestSetNonstrokingColorExtended(patternName, colors.ToArray());

    public virtual void TestSetNonstrokingColorExtended(PdfName? pdfName, double[] toArray)
    {
    }

    public virtual void SetStrokeGray(double grayLevel){}
    public virtual void SetStrokeRGB(double red, double green, double blue){}
    public virtual void SetStrokeCMYK(double cyan, double magenta, double yellow, double black){}
    public virtual void SetNonstrokingGray(double grayLevel){}
    public virtual void SetNonstrokingRGB(double red, double green, double blue){}
    public virtual void SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black){}

}