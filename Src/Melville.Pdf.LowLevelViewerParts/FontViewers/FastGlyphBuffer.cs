using System.Numerics;
using Melville.Fonts;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers
{
    internal readonly struct FastGlyphBuffer : IGlyphTarget
    {
        private readonly List<Vector2> points = new();
        private readonly List<GlyphOperation> operations = new();

        public FastGlyphBuffer()
        {
        }

        public void MoveTo(Vector2 point)
        {
            points.Add(point);
            operations.Add(GlyphOperation.MoveTo);
        }

        public void LineTo(Vector2 point)
        {
            points.Add(point);
            operations.Add(GlyphOperation.LineTo);
        }

        public void CurveTo(Vector2 control, Vector2 endPoint)
        {
            points.Add(control);
            points.Add(endPoint);
            operations.Add(GlyphOperation.QuadCurveTo);
        }

        public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint)
        {
            points.Add(control1);
            points.Add(control2);
            points.Add(endPoint);
            operations.Add(GlyphOperation.CubicCurveTo);
        }

        public void EndGlyph()
        {
        }

        public void Replay<T>(T target) where T:IGlyphTarget =>
            new GlyphReplayer<T>(points, operations, target).Replay();
    }
}

public enum GlyphOperation: byte
{
    MoveTo,
    LineTo,
    QuadCurveTo,
    CubicCurveTo
}

internal ref struct GlyphReplayer<T>(
    List<Vector2> points, 
    List<GlyphOperation> operations, 
    T target) where T:IGlyphTarget
{
    private int pointIndex = 0;

    public void Replay()
    {
        foreach (var operation in operations)
        {
            pointIndex += DoOperation(operation);
        }
    }

    private int DoOperation(GlyphOperation operation) => operation switch
    {
        GlyphOperation.MoveTo => DoMoveTo(),
        GlyphOperation.LineTo => DoLineTo(),
        GlyphOperation.QuadCurveTo => DoQuadCurveTo(),
        GlyphOperation.CubicCurveTo => DoCubicCurveTo(),
        _ => throw new ArgumentOutOfRangeException()
    };

    private int DoMoveTo()
    {
        target.MoveTo(points[pointIndex]);
        return 1;
    }

    private int DoLineTo()
    {
        target.LineTo(points[pointIndex]);
        return 1;
    }

    private int DoQuadCurveTo()
    {
        target.CurveTo(points[pointIndex], points[pointIndex + 1]);
        return 2;
    }

    private int DoCubicCurveTo()
    {
        target.CurveTo(points[pointIndex], points[pointIndex + 1], points[pointIndex + 2]);
        return 3;
    }
}
