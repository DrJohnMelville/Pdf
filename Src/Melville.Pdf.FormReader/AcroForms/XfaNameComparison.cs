namespace Melville.Pdf.FormReader.AcroForms;

/// <summary>
/// Xfa names can optionally have a [0] at the end of each name component which the spec
/// allows to be omitted.  This class compares characer spans while ignoring these optional
/// elements.
/// </summary>
public static unsafe class XfaNameComparison
{
    /// <summary>
    /// Compare two ReadOnlySpans while ignoring optional elements of XFA name syntax.
    /// </summary>
    /// <param name="spanA">The first name to compare</param>
    /// <param name="spanB">The name to compare the first to</param>
    /// <returns>True if the names refer to the same XFA object, false otherwise.</returns>
    public static bool SameXfaNameAs(this in ReadOnlySpan<char> spanA, in ReadOnlySpan<char> spanB)
    {
        // This string compare method ignores [0] before a . or the end of the string
        fixed(char* aStart = spanA)
        fixed (char* bStart = spanB)
        {
            char* aEnd = aStart + spanA.Length;
            char* bEnd = bStart + spanB.Length;
            char* a = aStart;
            char* b = bStart;

            for (; a < aEnd && b < bEnd; a++, b++)
            {
                if (*a == *b) continue;
                if (*a == '.' && SkipOverBracketZero(ref b, bEnd)) continue;
                if (*b == '.' && SkipOverBracketZero(ref a, aEnd)) continue;
                return false;
            }

            if ((a == aEnd) && IsTerminalBracket(b, bEnd)) return true;
            if ((b == bEnd) && IsTerminalBracket(a, aEnd)) return true;

            return (a == aEnd) && (b == bEnd);
        }
    }

    private static bool IsTerminalBracket(char* position, char* end) =>
        (end - position) == 3 &&
        *(position++) == '[' &&
        *(position++) == '0' &&
        *(position++) == ']'; 
    
    private static bool SkipOverBracketZero(ref char* position, char* end) =>
        (end - position) > 3 &&
        *(position++) == '[' &&
        *(position++) == '0' &&
        *(position++) == ']' &&
        *(position) == '.';
}