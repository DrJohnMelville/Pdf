using System.Numerics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.TextExtractor;

public interface IExtractedTextTarget
{
    void BeginWrite(IRealizedFont font);
    void EndWrite(Matrix3x2 textMatrix);
    void WriteCharacter(char character, Matrix3x2 textMatrix);
}

[FromConstructor]
public partial class ExtractTextRender :
    RenderTargetBase<IExtractedTextTarget, UncoloredGraphicsState>
{

    /// <inheritdoc />
    public override IDrawTarget CreateDrawTarget() => NullDrawTarget.Instance;

    /// <inheritdoc />
    public override ValueTask RenderBitmapAsync(IPdfBitmap bitmap) => ValueTask.CompletedTask;

    /// <inheritdoc />
    public override void SetBackgroundRect(
        in PdfRect rect, double width, double height, in Matrix3x2 transform)
    {
    }

    /// <inheritdoc />
    public override IRealizedFont WrapRealizedFont(IRealizedFont font) => 
        new ExtractingFont(font, Target);
}