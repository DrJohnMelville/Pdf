using System.IO.Pipelines;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
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

    public void SetBuiltInFont(PdfName name, double size);
    (double width, double height) RenderGlyph(byte b);
}

public abstract class RenderTargetBase<T, TTypeface>
{
    protected T Target { get; }
    protected GraphicsStateStack<TTypeface> State { get; }
    protected PdfPage Page { get; }
    protected IDefaultFontMapper DefaultFontMapper { get; }

    public IGraphiscState<TTypeface> GrapicsStateChange => State;

    protected RenderTargetBase(
        T target, GraphicsStateStack<TTypeface> state, PdfPage page, IDefaultFontMapper defaultFontMapper)
    {
        Target = target;
        State = state;
        Page = page;
        DefaultFontMapper = defaultFontMapper;
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

    protected abstract void SetBuiltInTypeface(DefaultPdfFonts name, bool bold, bool oblique);

    public void SetBuiltInFont(PdfName name, double size)
    {
        switch (name.GetHashCode())
        {
            case KnownNameKeys.Courier:
                SetBuiltInTypeface(DefaultPdfFonts.Courier, false, false);
                break;
            case KnownNameKeys.CourierBold:
                SetBuiltInTypeface(DefaultPdfFonts.Courier, true, false);
                break;
            case KnownNameKeys.CourierOblique:
                SetBuiltInTypeface(DefaultPdfFonts.Courier, false, true);
                break;
            case KnownNameKeys.CourierBoldOblique:
                SetBuiltInTypeface(DefaultPdfFonts.Courier, true, true);
                break;
            case KnownNameKeys.Helvetica:
                SetBuiltInTypeface(DefaultPdfFonts.Helvetica, false, false);
                break;
            case KnownNameKeys.HelveticaBold:
                SetBuiltInTypeface(DefaultPdfFonts.Helvetica, true, false);
                break;
            case KnownNameKeys.HelveticaOblique:
                SetBuiltInTypeface(DefaultPdfFonts.Helvetica, false, true);
                break;
            case KnownNameKeys.HelveticaBoldOblique:
                SetBuiltInTypeface(DefaultPdfFonts.Helvetica, true, true);
                break;
            case KnownNameKeys.TimesRoman:
                SetBuiltInTypeface(DefaultPdfFonts.Times, false, false);
                break;
            case KnownNameKeys.TimesBold:
                SetBuiltInTypeface(DefaultPdfFonts.Times, true, false);
                break;
            case KnownNameKeys.TimesOblique:
                SetBuiltInTypeface(DefaultPdfFonts.Times, false, true);
                break;
            case KnownNameKeys.TimesBoldOblique:
                SetBuiltInTypeface(DefaultPdfFonts.Times, true, true);
                break;
            case KnownNameKeys.Symbol:
                SetBuiltInTypeface(DefaultPdfFonts.Symbol, false, false);
                break;
            case KnownNameKeys.ZapfDingbats:
                SetBuiltInTypeface(DefaultPdfFonts.Dingbats, false, false);
                break;
            default: throw new PdfParseException("Cannot find builtin font: " + name);
        }
    }

    public (double width, double height) RenderGlyph(byte b)
    {
        if (State.Current().Typeface is not { } gtf) return (0, 0);
        return RenderGlyph(gtf, State.CurrentState().ByteMapper.MapToUnicode(b));
    }

    protected abstract (double width, double height) RenderGlyph(TTypeface gtf, char charInUnicode);

    protected Matrix3x2 CharacterPositionMatrix()
    {
        return (new Matrix3x2(
                    (float)State.CurrentState().HorizontalTextScale/100,0,
                    0,-1,
                    0, (float)State.CurrentState().TextRise) *
                State.CurrentState().TextMatrix);
    }

    #endregion
}

public static class RenderTargetOperations
{
    public static async ValueTask RenderTo<TTypeface>(
        this IHasPageAttributes page, IRenderTarget<TTypeface> target) =>
        await new ContentStreamParser(
                new RenderEngine<TTypeface>(page, target))
            .Parse(
                PipeReader.Create(
                    await page.GetContentBytes()));
}