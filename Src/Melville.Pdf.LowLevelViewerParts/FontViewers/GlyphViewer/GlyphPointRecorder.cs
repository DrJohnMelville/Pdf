using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer
{
    public class GlyphPointRecorder : ITrueTypePointTarget
    {
        public List<GlyphPointViewModel> Points { get; } = new();

        public void AddPoint(double x, double y, bool onCurve, bool isContourStart, bool isContourEnd)
        {
            Points.Add(new(x, y, StatusString(onCurve, isContourStart, isContourEnd)));
        }

        private string StatusString(bool onCurve, bool isContourStart, bool isContourEnd) =>
            (onCurve ? "On " : "") + (isContourStart ? "Start" : "") + (isContourEnd ? "End" : "");

        public void BeginGlyph(
            int level, short minX, short minY, short maxX, short maxY, in Matrix3x2 transform)
        {
            Points.Add(new(level, -1, "Begin Glyph"));
            BoundingPoint(minX, minY, transform, $"Lower Left");
            BoundingPoint(maxX, minY, transform, $"Lower Right");
            BoundingPoint(maxX, maxY, transform, $"Upper Right");
            BoundingPoint(minX, maxY, transform, $"Upper Left");
        }

        private void BoundingPoint(short minX, short minY, Matrix3x2 transform, string label)
        {
            var location = Vector2.Transform(new(minX, minY), transform);
            Points.Add(new(location.X, location.Y, label));
        }

        public void EndGlyph(int level)
        {
            Points.Add(new(level, -1, "End Glyph"));
        }
    }
}