using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

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
    unsafe ValueTask RenderPbgraAsync(byte* buffer);
}

/// <summary>
/// This contains methods to render the bitmap to a C# array of bytes
/// </summary>
public static class RenderToArrayImpl
{
    /// <summary>
    /// Copies a bitmap to an appropriately sized C# array by pinning the array and rendering
    /// tp the resulting pointer.
    /// </summary>
    /// <param name="bitmap">Bitmap to render</param>
    /// <param name="target">array to fill with the bitmap bits</param>
    public static unsafe ValueTask CopyToArrayAsync(this IPdfBitmap bitmap, byte[] target)
    {
        Debug.Assert(target.Length >= bitmap.ReqiredBufferSize());
        var handle = GCHandle.Alloc(target, GCHandleType.Pinned);
        byte* pointer = (byte*)handle.AddrOfPinnedObject();
        return new(bitmap.RenderPbgraAsync(pointer).AsTask().ContinueWith(_ => handle.Free()));
    }

    /// <summary>
    /// Render the bitmap to a C# array
    /// </summary>
    /// <param name="bitmap">The bitmap to render.</param>
    /// <returns>A byte array containing the bitmap as pargb quadruples</returns>
    public static async ValueTask<byte[]> AsByteArrayAsync(this IPdfBitmap bitmap)
    {
        var ret = new byte[bitmap.ReqiredBufferSize()];
        await CopyToArrayAsync(bitmap, ret).CA();
        return ret;
    }
}