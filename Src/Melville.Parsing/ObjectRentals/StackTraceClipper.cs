using System.Diagnostics;
using System.Text;

namespace Melville.Parsing.ObjectRentals;

public static class StackTraceClipper
{
    public static string Clip(this StackTrace trace)
    {
        var frames = trace.GetFrames().Select(PrintFrame).ToArray();
        var last = 0;
        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i].Contains("Melville")) last = i;
        }

        var ret = new StringBuilder();
        for (int i = 0; i <= last; i++)
        {
            ret.AppendLine(frames[i]);
        }

        return ret.ToString();
    }

    private static string PrintFrame(StackFrame i)
    {
        var method = i.GetMethod();
        if (method is null) return "No Method";
        return $"{method.DeclaringType}.{method.Name}";
    }
}