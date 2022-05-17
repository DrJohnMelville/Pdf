using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public ref struct PrefixCode
{
    private int currentCode = 0;

    public int NextCode() => currentCode++;
    // the current code for the current code length becomes the prefix code for the next code
    // length, so a right shift gives us a code one longer, with the right prefix, and with
    // 0 in the new ones bit.
    public void IncrementCodeLength() => currentCode <<= 1;
}

public ref struct BitAssignment
{
    public static void AssignPrefixes(in ReadOnlySpan<int> prefixLengths, in Span<int> prefixes) => 
        new BitAssignment(prefixLengths, prefixes).AssignPrefixesForAllLengths();
    
    private readonly ReadOnlySpan<int> prefixLengths;
    private readonly Span<int> prefixes;
    private PrefixCode currentCode = new();

    private BitAssignment(in ReadOnlySpan<int> prefixLengths, in Span<int> prefixes)
    {
        Debug.Assert(prefixLengths.Length == prefixes.Length);
        this.prefixLengths = prefixLengths;
        this.prefixes = prefixes; 
    }

    private const int MinimumPossibleCodeLength = 1;
    private void AssignPrefixesForAllLengths()
    {
        var maximumCodeLength = GetMaxCodeLength(prefixLengths);
        for (var curLen = MinimumPossibleCodeLength; curLen <= maximumCodeLength ; curLen++)
        {
            currentCode.IncrementCodeLength();
            AssignPrefixesOfLength(curLen);
        }
    }

    private static int GetMaxCodeLength(in ReadOnlySpan<int> prefixLengths)
    {
        var maximum = 0;
        foreach (var prefixLength in prefixLengths) maximum = Math.Max(prefixLength, maximum);
        return maximum;
    }

    private  void AssignPrefixesOfLength(int selectedLength)
    {
        for (var i = 0; i < prefixLengths.Length; i++)
        {
            if (prefixLengths[i] == selectedLength)
            {
                prefixes[i] = currentCode.NextCode();
            }
        }
    }
}