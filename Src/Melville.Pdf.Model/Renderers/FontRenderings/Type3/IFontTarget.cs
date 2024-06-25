using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

/// <summary>
/// This is the target of font writing operation.
/// </summary>
public interface IFontTarget
{
    /// <summary>
    /// Render the type 3 font character represented by the given matrix and font dictionary
    /// </summary>
    /// <param name="s">A content stream representing the character.</param>
    /// <param name="fontMatrix">The font matrix</param>
    /// <param name="fontDictionary">The dictionary defining the font -- which may contain resources.</param>
    /// <returns>A valuetask containing the width of the rendered character.</returns>
    ValueTask<double> RenderType3CharacterAsync(Stream s, Matrix3x2 fontMatrix, PdfDictionary fontDictionary);
    /// <summary>
    /// Create a IDrawTarget that the stroked character can be drawn to.
    /// </summary>
    IDrawTarget CreateDrawTarget();

    /// <summary>
    /// The render target that will eventually be drawn to.
    /// Fonts can use this to short circut the drawing.  (For example see Melville.Pdf.TextExtractor.ExtractingFont)
    /// </summary>
    IRenderTarget RenderTarget { get; }
}