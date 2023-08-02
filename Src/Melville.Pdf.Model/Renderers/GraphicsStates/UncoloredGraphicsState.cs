using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.DocumentRenderers;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// This is a GraphicsState onject for renderers that do not have a concept of color.
/// It does not keep copies of the current brush.
/// 
/// </summary>
public class UncoloredGraphicsState : GraphicsState
{
    /// <inheritdoc />
    protected override void StrokeColorChanged()
    {
    }

    /// <inheritdoc />
    protected override void NonstrokeColorChanged()
    {
    }

    /// <inheritdoc />
    public override ValueTask SetStrokePatternAsync(PdfDictionary pattern, DocumentRenderer parentRenderer) =>
        ValueTask.CompletedTask;

    /// <inheritdoc />
    public override ValueTask SetNonstrokePatternAsync(PdfDictionary pattern, DocumentRenderer parentRenderer) =>
        ValueTask.CompletedTask;
}