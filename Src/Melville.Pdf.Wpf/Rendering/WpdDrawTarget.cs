using System.Numerics;
using System.Windows.Media;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf.Rendering;

internal class WpfDrawTarget : WpfPathCreator
{
    private readonly DrawingContext context;
    private readonly GraphicsStateStack<WpfGraphicsState> state;
    private readonly GeometryGroup geoGroup = new GeometryGroup();
  
    public WpfDrawTarget(DrawingContext context, GraphicsStateStack<WpfGraphicsState> state)
    {
        this.context = context;
        this.state = state;
    }

    public override PathGeometry RequireGeometry()
    {
        if (Geometry is null) geoGroup.Children.Add(base.RequireGeometry());
        return Geometry;
    }

    public override void SetDrawingTransform(in Matrix3x2 transform)
    {
        SetGeometry(new PathGeometry() { Transform = transform.WpfTransform() });
        geoGroup.Children.Add(Geometry);
    }

    public void AddGeometry(PathGeometry geometry)
    {
        SetGeometry(geometry);
        geoGroup.Children.Add(Geometry);
    }

    public override void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        SetCurrentFillRule(evenOddFillRule);
        InnerPathPaint(stroke, fill, geoGroup);
    }

    private void InnerPathPaint(bool stroke, bool fill, Geometry pathToPaint) =>
        context.DrawGeometry(
            fill ? state.StronglyTypedCurrentState().Brush() : null, 
            stroke ? state.StronglyTypedCurrentState().Pen() : null, 
            pathToPaint);

    private void SetCurrentFillRule(bool evenOddFillRule) =>
        geoGroup.FillRule = evenOddFillRule ? FillRule.EvenOdd : FillRule.Nonzero;

    public override void ClipToPath(bool evenOddRule)
    {
        SetCurrentFillRule(evenOddRule);
        state.StronglyTypedCurrentState().WpfStackframesPushed++;
        context.PushClip(geoGroup);
    }
}