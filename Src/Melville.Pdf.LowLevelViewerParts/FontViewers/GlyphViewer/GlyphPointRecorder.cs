using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.GlyphViewer
{
    [Obsolete("Make this go away")]
    public class GlyphPointRecorder : ITrueTypePointTarget
    {
        public List<GlyphPointViewModel> Points { get; } = new();

        public void AddPoint(Vector2 point, bool onCurve, bool isContourStart, bool isContourEnd)
        {
            Points.Add(new(point.X, point.Y, StatusString(onCurve, isContourStart, isContourEnd)));
        }

        private string StatusString(bool onCurve, bool isContourStart, bool isContourEnd) =>
            (onCurve ? "On " : "") + (isContourStart ? "Start" : "") + (isContourEnd ? "End" : "");

        public void AddPhantomPoint(Vector2 point)
        {
        }
    }
}