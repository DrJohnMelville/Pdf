using System;
using System.IO.Pipelines;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

public interface IRenderTarget<TTypeface>
{
    IGraphiscState<TTypeface> GrapicsStateChange { get; }
    void MoveTo(double x, double y);
    void LineTo(double x, double y);

    void CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY);

    void ClosePath();
    void PaintPath(bool stroke, bool fill, bool evenOddFillRule);
    void EndPath();

    void SaveTransformAndClip();
    void RestoreTransformAndClip();
    void Transform(in Matrix3x2 newTransform);
    void CombineClip(bool evenOddRule);

    ValueTask RenderBitmap(IPdfBitmap bitmap);

    public void SetFont(IFontMapping font, double size);
    (double width, double height) AddGlyphToCurrentString(byte b);
    void RenderCurrentString();
}

public abstract class RenderTargetBase<T, TTypeface>
{
    protected T Target { get; }
    protected GraphicsStateStack<TTypeface> State { get; }
    protected PdfPage Page { get; }
 
    public IGraphiscState<TTypeface> GrapicsStateChange => State;

    protected RenderTargetBase(T target, GraphicsStateStack<TTypeface> state, PdfPage page)
    {
        Target = target;
        State = state;
        Page = page;
    }

    protected void MapUserSpaceToBitmapSpace(PdfRect rect, double xPixels, double yPixels)
    {
        var xform = Matrix3x2.CreateTranslation((float)-rect.Left, (float)-rect.Bottom) *
                    Matrix3x2.CreateScale((float)(xPixels / rect.Width), (float)(-yPixels / rect.Height)) *
                    Matrix3x2.CreateTranslation(0, (float)yPixels);
        State.ModifyTransformMatrix(xform);
        Transform(xform);
    }

    public abstract void Transform(in Matrix3x2 newTransform);

    #region TextRendering
    public (double width, double height) AddGlyphToCurrentString(byte b)
    {
        if (State.Current().Typeface is not { } gtf) return (0, 0);
        return AddGlyphToCurrentString(gtf, State.CurrentState().ByteMapper.MapToUnicode(b));
    }
    protected abstract (double width, double height) AddGlyphToCurrentString(TTypeface gtf, char charInUnicode);

    public void RenderCurrentString()
    {
        var textRender = State.CurrentState().TextRender;
        RenderCurrentString(
            textRender.ShouldStroke(), textRender.ShouldFill(), textRender.ShouldClip());
    }

    protected abstract void RenderCurrentString(bool stroke, bool fill, bool clip);

    protected Matrix3x2 CharacterPositionMatrix() =>
        (GlyphAdjustmentMatrix() *
         State.CurrentState().TextMatrix);

    private Matrix3x2 GlyphAdjustmentMatrix() => new(
            (float)State.CurrentState().HorizontalTextScale/100,0,
            0,-1,
            0, (float)State.CurrentState().TextRise);

    #endregion

    #region Font mapping
    protected static int CommonPrefixLength(byte[] fontName, string familySource)
    {
        var fontNamePos = 0;
        var familyPos = 0;
        while (true)
        {
            if (fontNamePos >= fontName.Length || familyPos >= familySource.Length)
                return fontNamePos;
            switch ((fontName[fontNamePos], (byte)(familySource[familyPos])))
            {
                case (32, _) : fontNamePos++; break;
                case (_, 32) : familyPos++; break;
                case var(a,b) when a==b:
                    fontNamePos++;
                    familyPos++;
                    break;
                default: return fontNamePos;
            }
        }
    }
    #endregion
}

public static class RenderTargetOperations
{
    public static async ValueTask RenderTo<TTypeface>(
        this IHasPageAttributes page, IRenderTarget<TTypeface> target, FontReader fonts) =>
        await new ContentStreamParser(
                new RenderEngine<TTypeface>(page, target, fonts))
            .Parse(
                PipeReader.Create(
                    await page.GetContentBytes()));
}