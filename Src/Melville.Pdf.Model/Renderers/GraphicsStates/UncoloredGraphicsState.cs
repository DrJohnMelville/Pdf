using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.DocumentRenderers;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// This is a GraphicsState onject for renderers that do not have a concept of color.
/// It does not keep copies of the current brush.
/// 
/// </summary>
public class UncoloredGraphicsState() : GraphicsState(
    UncoloredNativeBrush.Instance, UncoloredNativeBrush.Instance)
{
}

[StaticSingleton]
public partial class UncoloredNativeBrush: INativeBrush
{
    /// <inheritdoc />
    public void SetSolidColor(DeviceColor color)
    {
    }

    /// <inheritdoc />
    public void SetAlpha(double alpha)
    {
    }

    /// <inheritdoc />
    public ValueTask SetPatternAsync(
        PdfDictionary pattern, DocumentRenderer parentRenderer,
        GraphicsState prior) => ValueTask.CompletedTask;

    /// <inheritdoc />
    public void Clone(INativeBrush target)
    {
    }

    /// <inheritdoc />
    public T TryGetNativeBrush<T>() => default;
}