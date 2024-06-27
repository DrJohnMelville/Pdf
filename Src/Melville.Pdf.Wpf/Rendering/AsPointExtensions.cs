using System.Numerics;
using System.Windows;

namespace Melville.Pdf.Wpf.Rendering;

internal static class AsPointExtensions
{
    public static Point AsPoint(this Vector2 vec) => new Point(vec.X, vec.Y);
}