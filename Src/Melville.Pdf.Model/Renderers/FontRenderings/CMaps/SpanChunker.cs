using System;
using System.Diagnostics;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

public static class SpanChunker
{
    public static void ForEachGroup<T>(
        this ReadOnlySpan<T> values, Action<T, T> body)
    {
        Debug.Assert(values.Length % 2 == 0);
        for (int i = 0; i < values.Length; i+=2)
        {
            body(values[i],values[i + 1]);
        }
        
    }
    public static void ForEachGroup<T>(
        this ReadOnlySpan<T> values, Action<T, T, T> body)
    {
        Debug.Assert(values.Length % 3 == 0);
        for (int i = 0; i < values.Length; i+=3)
        {
            body(values[i],values[i + 1],values[i + 2]);
        }
        
    }
}