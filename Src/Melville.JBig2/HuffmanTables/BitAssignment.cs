using System;
using System.Diagnostics;

namespace Melville.JBig2.HuffmanTables;

public ref struct PrefixCode
{
    private short currentCode = 0;
    public short CodeLength { get; set; } = 0;

    public PrefixCode() 
    {
    }

    public int NextCode()
    {
        return currentCode++;
    }

    // the current code for the current code length becomes the prefix code for the next code
    // length, so a right shift gives us a code one longer, with the right prefix, and with
    // 0 in the new ones bit.
    public void IncrementCodeLength()
    {
        CodeLength++;
        currentCode <<= 1;
    }
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

    private void AssignPrefixesForAllLengths()
    {
        do
        {
            currentCode.IncrementCodeLength();
        } while (AssignPrefixesOfLength());
    }
    
    private bool AssignPrefixesOfLength()
    {
        var greaterLengthCodesExist = false; 
        for (var i = 0; i < prefixLengths.Length; i++)
        {
            switch (prefixLengths[i] - currentCode.CodeLength)
            {
                case < 0: break;
                case 0:
                    prefixes[i] = currentCode.NextCode();
                    break;
                default:
                    greaterLengthCodesExist = true;
                    break;
            }
        }

        return greaterLengthCodesExist;
    }
}