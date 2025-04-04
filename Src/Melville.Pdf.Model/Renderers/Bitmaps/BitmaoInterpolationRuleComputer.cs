using System.Numerics;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

/// <summary>
/// Determine if an IPdfBitmap should be drawn with Interpolation
/// </summary>
public static class BitmaoInterpolationRuleComputer
{
    /// <summary>
    /// Apply all the PDF rules to determine if an image should be interpolated when painting
    /// </summary>
    /// <param name="bitmap">The bitmap to be painted.</param>
    /// <param name="currentTransform">The current view transform.</param>
    /// <returns></returns>
    public static bool ShouldInterpolate(this IPdfBitmap bitmap,
        in Matrix3x2 currentTransform) =>
        bitmap.DeclaredWithInterpolation || IsDowmScale(bitmap, currentTransform);
    
    private static bool IsDowmScale(IPdfBitmap bitmap, in Matrix3x2 transform)
    {
        var origin =  CurrentSpaceToFinalSpace(0, 0, transform);
        var lowerRight = CurrentSpaceToFinalSpace(1, 0, transform);
        var upperLeft = CurrentSpaceToFinalSpace(0, 1, transform);
        return (upperLeft - origin).Length() <= bitmap.Height ||
               (lowerRight - origin).Length() <= bitmap.Width;
    }

    private static Vector2 CurrentSpaceToFinalSpace(float x, float y, in Matrix3x2 transform) => 
        Vector2.Transform(new Vector2(x,y), transform);

         
}