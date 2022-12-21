using System.Text;


namespace Melville.Icc.Model;

internal static class Visualizations
{
    public static string As4CC(uint source) =>
        Encoding.UTF8.GetString(stackalloc byte[]
        {
            (byte)(source >> 24),
            (byte)(source >> 16),
            (byte)(source >> 8),
            (byte)(source)
        });
}