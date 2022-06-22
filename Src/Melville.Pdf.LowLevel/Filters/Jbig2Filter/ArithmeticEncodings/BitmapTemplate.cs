using System;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public readonly record struct ContextBitRun(sbyte X, sbyte Y, byte Length, byte MinBit)
{
    public int NextBit() => MinBit + Length;

    public bool IsLastRunInThisBitmap() => MinBit == 0;
}

public readonly struct BitmapTemplate
{
    private readonly ContextBitRun[] runs;
    public int BitsRequired() => runs[0].NextBit();

    public BitmapTemplate(ContextBitRun[] runs)
    {
        this.runs = runs;
    }

    public int ReadContext(IBinaryBitmap bitmap, int row, int col, int ret = 0)
    {
        foreach (var run in runs)
        {
            var runPtr = bitmap.PointerFor(row + run.Y, col + run.X);
            for (int i = 0; i < run.Length; i++)
            {
                ret <<= 1;
                ret |= runPtr.CurrentValue;
                runPtr.Increment();
            }
        }

        return ret;
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