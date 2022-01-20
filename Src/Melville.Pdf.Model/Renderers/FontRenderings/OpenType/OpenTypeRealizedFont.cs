using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Hacks.Reflection;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using SixLabors.Fonts;

namespace Melville.Pdf.Model.Renderers.FontRenderings.OpenType;

public class OpenTypeRealizedFont: IRealizedFont
{
    private readonly IFontTarget target;
    private double size;
    private readonly IByteToUnicodeMapping charMap;
    private readonly Font font;

    public OpenTypeRealizedFont(IFontTarget target, double size, IByteToUnicodeMapping charMap, Font font)
    { 
        this.target = target;
        this.size = size;
        this.charMap = charMap;
        this.font = font;
    }

    public IFontWriteOperation BeginFontWrite() => new FontWriteOperation(target.CreateDrawTarget(), this);

    private class FontWriteOperation : IFontWriteOperation, IGlyphRenderer
    {
        private readonly IDrawTarget target;
        private readonly OpenTypeRealizedFont parent;

        public FontWriteOperation(IDrawTarget target, OpenTypeRealizedFont parent)
        {
            this.target = target;
            this.parent = parent;
        }

        public ValueTask<(double width, double height)> AddGlyphToCurrentString(byte b, Matrix3x2 textMatrix)
        {
            target.SetDrawingTransform(textMatrix);
            var unicodePoint = parent.charMap.MapToUnicode(b);
            var glyph = parent.font.GetGlyph(unicodePoint);
            if (!Char.IsWhiteSpace(unicodePoint))
            {
                glyph.Instance.RenderTo(this, (float)parent.size, 
                    new Vector2(), new Vector2(72,72), (float)parent.size);
            }
            var fontScale = parent.size / glyph.Instance.SizeOfEm;
            return ValueTask.FromResult(
                ((double)glyph.Instance.AdvanceWidth * fontScale, 
                    (double)glyph.Instance.Height * fontScale));
        }

        public void RenderCurrentString(bool stroke, bool fill, bool clip)
        {
            if (stroke || fill)
            {
                target.PaintPath(stroke, fill, false);
            }

            if (clip)
            {
                target.ClipToPath(false);
            }
        }

        #region Forward glyph drawing commands to the DrawTarget

        public void MoveTo(Vector2 point) => target.MoveTo(point.X, point.Y);

        public void QuadraticBezierTo(Vector2 secondControlPoint, Vector2 point)
        {
            target.CurveTo(secondControlPoint.X, secondControlPoint.Y,
                point.X, point.Y,
                point.X, point.Y); 
        }

        public void CubicBezierTo(Vector2 secondControlPoint, Vector2 thirdControlPoint, Vector2 point)
        {
            target.CurveTo(secondControlPoint.X, secondControlPoint.Y,
                thirdControlPoint.X, thirdControlPoint.Y,
                point.X, point.Y); 
        }

        public void LineTo(Vector2 point)
        {
            target.LineTo(point.X, point.Y);
        }

        #endregion

        #region Begin and End Methods

        public void BeginText(FontRectangle bounds)
        {
        }

        public bool BeginGlyph(FontRectangle bounds, GlyphRendererParameters paramaters)
        {
            return true;
        }

        public void BeginFigure()
        {
        }

        public void EndFigure()
        {
        }

        public void EndGlyph()
        {
        }

        public void EndText()
        {
        }

        #endregion
    }
}