﻿using System;

namespace Melville.JBig2.ArithmeticEncodings;

internal readonly record struct ContextBitRun(sbyte X, sbyte Y, byte Length, byte MinBit)
{
    public int NextBit() => MinBit + Length;

    public bool IsLastRunInThisBitmap() => MinBit == 0;
}

internal readonly struct BitmapTemplate
{
    private readonly ContextBitRun[] runs;
    public int BitsRequired() => runs[0].NextBit();

    public BitmapTemplate(ContextBitRun[] runs)
    {
        this.runs = runs;
    }

    public IncrementalTemplate ToIncrementalTemplate() => new(runs.AsSpan());

    public ContextBitRun[] JoinRunsWith(BitmapTemplate other)
    {
        var ret = new ContextBitRun[runs.Length + other.runs.Length];
        runs.AsSpan().CopyTo(ret.AsSpan());
        other.runs.AsSpan().CopyTo(ret.AsSpan(runs.Length));
        return ret;
    }

}