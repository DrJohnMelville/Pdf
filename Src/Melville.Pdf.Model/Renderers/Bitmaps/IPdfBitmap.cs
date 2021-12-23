using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IPdfBitmap
{
    int Width { get; }
    int Height { get; }

    /// <summary>
    /// Fill the bitmap pointed to by buffer.
    /// This is implemented as an unsafe pointer operation so that I can quickly fill native buffers,
    /// deep in the graphics stack.
    /// </summary>
    /// <param name="buffer">A byte pointer which must point to the beginning of a buffer that
    /// is Width * Height *4 bytes long</param>
    unsafe ValueTask RenderPbgra(byte* buffer);
}