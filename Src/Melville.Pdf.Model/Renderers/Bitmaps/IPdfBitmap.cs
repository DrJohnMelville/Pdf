using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces;

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
    ValueTask RenderPbgraAsync(Memory<DeviceColor> buffer);
}

/// <summary>
/// This contains methods to render the bitmap to a C# array of bytes
/// </summary>
public static class RenderToArrayImpl
{
    /// <summary>
    /// Render the bitmap to a C# array
    /// </summary>
    /// <param name="bitmap">The bitmap to render.</param>
    /// <returns>A byte array containing the bitmap as argb quadruples</returns>
    public static async ValueTask<DeviceColor[]> AsDeviceColorArrayAsync(this IPdfBitmap bitmap)
    {
        var ret = new DeviceColor[bitmap.TotalPixels()];
        await bitmap.RenderPbgraAsync(ret).CA();
        return ret;
    }
}
