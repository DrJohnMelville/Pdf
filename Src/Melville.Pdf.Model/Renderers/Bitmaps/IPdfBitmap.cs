using System.Numerics;
using System.Threading.Tasks;
using SharpFont.PostScript;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IPdfBitmap
{
    int Width { get; }
    int Height { get; }
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

public static class PdfBitmapOperations
{
    public static bool ShouldInterpolate(this IPdfBitmap bitmap,
        in Matrix3x2 currentTransform) =>
        bitmap.DeclaredWithInterpolation || IsDowmScale(bitmap, currentTransform);
    
    private static bool IsDowmScale(IPdfBitmap bitmap, in Matrix3x2 transform)
    {
        var origin =  CurrentSpaceToFinalSpace(0, 0, transform);
        var lowerRight = CurrentSpaceToFinalSpace(1, 0, transform);
        var upperLeft = CurrentSpaceToFinalSpace(0, 1, transform);
        return (upperLeft - origin).Length() < bitmap.Height &&
               (lowerRight - origin).Length() < bitmap.Width;
    }

    private static Vector2 CurrentSpaceToFinalSpace(float x, float y, in Matrix3x2 transform) => 
        Vector2.Transform(new Vector2(x,y), transform);

         
}