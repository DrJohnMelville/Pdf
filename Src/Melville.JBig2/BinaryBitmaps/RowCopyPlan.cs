﻿
using Melville.JBig2.Segments;

namespace Melville.JBig2.BinaryBitmaps;

internal readonly  struct RowCopyPlan
{
    public readonly int FirstSourceBit;
    public readonly int FirstDestBit;
    public readonly int WholeBytes;
    public readonly int SuffixBits;
    public readonly CombinationOperator CombinationOperator;
    public readonly ByteSplicer PrefixSplicer;
    public readonly ByteSplicer PostSplicer;

    public RowCopyPlan(int firstSourceBit, int firstDestBit, int wholeBytes, int suffixBits, CombinationOperator combinationOperator)
    {
        FirstSourceBit = firstSourceBit;
        FirstDestBit = firstDestBit;
        WholeBytes = wholeBytes;
        SuffixBits = suffixBits;
        CombinationOperator = combinationOperator;
        PrefixSplicer = new ByteSplicer(FirstDestBit);
        PostSplicer = new ByteSplicer(SuffixBits);
    }
}