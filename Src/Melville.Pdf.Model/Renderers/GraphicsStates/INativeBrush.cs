using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.DocumentRenderers;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

/// <summary>
/// This represents the PDF concept of a brush (stroking or nonstroking) except
/// that it is specialized to a rendering technology.
/// </summary>
public interface INativeBrush
{
    /// <summary>
    /// Called when the PDF content stream overwrites the current brush with
    /// a new solid color brush.
    /// </summary>
    /// <param name="color">The device color of the new brush.</param>
    void SetSolidColor(DeviceColor color);

    /// <summary>
    /// Called when a PDF content stream sets the alpha of the current brush.
    /// </summary>
    /// <param name="alpha">The desired alpha value</param>
    void SetAlpha(double alpha);

    /// <summary>
    /// Called when the PDF content stream sets a brush to a pattern.
    /// </summary>
    /// <param name="pattern">The pattern to set the brush to.</param>
    /// <param name="parentRenderer">The parent renderer that provides context to
    ///     this pattern.
    /// </param>
    /// <param name="prior">The graphics state that request
    /// came from.</param>
    ValueTask SetPatternAsync(
        PdfDictionary pattern, DocumentRenderer parentRenderer,
        GraphicsState prior);
    
    /// <summary>
    /// Clone this brush into an equivilent brush. (Used when pushing context.)
    /// </summary>
    void Clone(INativeBrush target);

    /// <summary>
    /// Try to create a native brush of typw T corresponding to this brush.
    /// May thrw any exception if T is not the type of native brush I expect to
    /// create.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T TryGetNativeBrush<T>();
}