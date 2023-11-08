namespace Melville.Pdf.FormReader.AcroForms;

public static unsafe class XfaNameComparison
{
    public static bool SameXfaNameAs(this in ReadOnlySpan<char> fullName, in ReadOnlySpan<char> name)
    {
        // This string compare method ignores [0] before a . or the end of the string
        fixed(char* aStart = fullName)
        fixed (char* bStart = name)
        {
            char* aEnd = aStart + fullName.Length;
            char* bEnd = bStart + name.Length;
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