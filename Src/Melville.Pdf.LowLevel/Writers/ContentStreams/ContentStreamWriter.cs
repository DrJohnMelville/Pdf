using System;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

public class ContentStreamWriter : IContentStreamOperations
{
    private readonly ContentStreamPipeWriter destPipe;

    public ContentStreamWriter(PipeWriter destPipe)
    {
        this.destPipe = new ContentStreamPipeWriter(destPipe);
    }

    #region Graphic State Operations

    public void SaveGraphicsState() => destPipe.WriteOperator(ContentStreamOperatorNames.q);
    public void RestoreGraphicsState() => destPipe.WriteOperator(ContentStreamOperatorNames.Q);

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
}