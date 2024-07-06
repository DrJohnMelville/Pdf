using System.Diagnostics;

namespace Melville.Parsing.SpanAndMemory;

/// <summary>
/// This is the algorithm for the PostScript roll stack operation, which gets used
/// in multiple places in the PostScript interpreter and font parsers.
/// </summary>
public static class SpanRoller
{
    /// <summary>
    /// Rotate the top N elements of a span by a given number of places.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span</typeparam>
    /// <param name="span">The span to roll</param>
    /// <param name="places">Number of spaces to roll by</param>
    /// <param name="size">The roll should involve that last N items in the span</param>
    public static void Roll<T>(this Span<T> span, int places, int size) =>
        Roll(span[^size..], places);

    /// <summary>
    /// Rotate the top N elements of a span by a given number of places.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span</typeparam>
    /// <param name="span">The span to roll</param>
    /// <param name="places">Number of spaces to roll by</param>
    public static void Roll<T>(this Span<T> span, int places)
    {
        while (places < 0) places += span.Length; 
        places %= span.Length;

        Debug.Assert(places >= 0);
        Debug.Assert(places < span.Length);
    
        //Uses the reversalAlgorithm for array rotation.
        //https://www.geeksforgeeks.org/complete-guide-on-array-rotations/?ref=ml_lbp
        span[..^places].Reverse();
        span[^places..].Reverse();
        span.Reverse();
    }
}