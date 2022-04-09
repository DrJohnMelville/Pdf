
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Melville.Icc.Model;


namespace Melville.Icc.Model;

public static class Visualizations
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