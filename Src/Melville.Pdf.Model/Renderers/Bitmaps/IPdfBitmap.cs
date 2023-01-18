using System.Threading.Tasks;
using Melville.SharpFont.PostScript;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

/// <summary>
/// Represents a PDF sampled image
/// </summary>
public interface IPdfBitmap
{
    /// <summary>
    /// Width of the image in pixels
    /// </summary>
    int Width { get; }
    /// <summary>
    /// Height of the image in pixels
    /// </summary>
    int Height { get; }
    /// <summary>
    /// Indicates that the image is declared that it should use interpolation upon expanding the image.
    /// </summary>
    bool DeclaredWithInterpolation { get; }
    /// <summary>
    /// Fill the bitmap pointed to by buffer.
    /// This is implemented as an unsafe pointer operation so that I can quickly fill native buffers,
    /// deep in the graphics stack.
    /// </summary>
    /// <param name="buffer">A byte pointer which must point to the beginning of a buffer that
    /// is Width * Height *4 bytes long</param>
    unsafe ValueTask RenderPbgra(byte* buffer);
}