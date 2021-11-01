using System;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class ConcreteCSO: IContentStreamOperations
{
    public virtual void SaveGraphicsState()
    {
    }

    public virtual void RestoreGraphicsState()
    {
    }

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
}