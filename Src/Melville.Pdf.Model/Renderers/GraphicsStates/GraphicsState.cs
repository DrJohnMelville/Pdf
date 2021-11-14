using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public partial class GraphicsState: IStateChangingOperations
{
    [MacroItem("double", "LineWidth", "1.0")]
    [MacroItem("double", "MiterLimit", "10.0")]
    [MacroItem("LineJoinStyle", "LineJoinStyle", "LineJoinStyle.Miter")]
    [MacroItem("LineCap", "LineCap", "LineCap.Square")]
    [MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
    [MacroCode("    ~1~ = other.~1~;", Prefix = "public void CopyFrom(GraphicsState other){", Postfix = "}")]
    public void SaveGraphicsState() { }
    public void RestoreGraphicsState() { }

    public void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f)
    {
        throw new NotImplementedException();
    }

    public void SetLineWidth(double width) => LineWidth = width;
    public void SetLineCap(LineCap cap) => LineCap = cap;
    public void SetLineJoinStyle(LineJoinStyle lineJoinStyle) => LineJoinStyle = lineJoinStyle;
    public void SetMiterLimit(double miter) => MiterLimit = miter;

    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
        throw new NotImplementedException();
    }

    public void SetRenderIntent(RenderingIntentName intent)
    {
        throw new NotImplementedException();
    }

    public void SetFlatnessTolerance(double flatness)
    {
        throw new NotImplementedException();
    }

    public void LoadGraphicStateDictionary(PdfName dictionaryName)
    {
        throw new NotImplementedException();
    }

    public void SetCharSpace(double value)
    {
        throw new NotImplementedException();
    }

    public void SetWordSpace(double value)
    {
        throw new NotImplementedException();
    }

    public void SetHorizontalTextScaling(double value)
    {
        throw new NotImplementedException();
    }

    public void SetTextLeading(double value)
    {
        throw new NotImplementedException();
    }

    public void SetFont(PdfName font, double size)
    {
        throw new NotImplementedException();
    }

    public void SetTextRender(TextRendering rendering)
    {
        throw new NotImplementedException();
    }

    public void SetTextRise(double value)
    {
        throw new NotImplementedException();
    }
}