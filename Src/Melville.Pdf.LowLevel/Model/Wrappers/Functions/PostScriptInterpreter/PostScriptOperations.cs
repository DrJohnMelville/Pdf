using System;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

public static partial class PostScriptOperations
{
    private static double DegToRad(double angle) => angle * Math.PI / 180;
    private static double RadToDeg(double angle) => angle * 180 / Math.PI;
    private static double CanonicalDegrees(double angle) => (angle + 360.0) % 360.0;
    private static double PostscriptRound(double d)
    {
        var floor = Math.Round(d, MidpointRounding.ToNegativeInfinity);
        return d - floor >= 0.5 ? floor + 1 : floor;
    }

    private static long PostscriptBitShift(long val, int shift) => 
        shift >= 0 ? val << shift : val >> -shift;

    private static double PostscriptEqual(double a, double b) =>
        (Math.Abs(a - b) < 0.0001) ? -1.0 : 0.0;
    private static double PostscriptNotEqual(double a, double b) =>
        (Math.Abs(a - b) >= 0.0001) ? -1.0 : 0.0;

    private static void PostscriptCopy(PostscriptStack s)
    {
        int count = (int)s.Pop();
        if (count < 0) throw new PdfParseException("Cannot copy a negative amount");
        Span<double> buffer = s.AsSpan()[^count..];
        PushSpan(s, buffer);
    }
    private static void PushSpan(PostscriptStack s, in Span<double> buffer)
    {
        foreach (var item in buffer)
        {
            s.Push(item);
        }
    }

    private static void RollSpan(Span<double> span, int delta)
    {
        int initialSpot = delta > 0 ? 
            delta % span.Length : 
            span.Length - ((-delta) % span.Length) % span.Length;
        if (initialSpot == 0) return;
        Span<double> buffer = stackalloc double[span.Length];
        for (int i = 0; i < span.Length; i++)
        {
            buffer[(i + initialSpot) % span.Length] = span[i];
        }
        buffer.CopyTo(span);
    }
}