using System;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public partial struct ContextBitRun
{
    [FromConstructor] public readonly sbyte X;
    [FromConstructor] public readonly sbyte Y;
    [FromConstructor] public readonly byte Length;
    [FromConstructor] public readonly byte MinBit;
    public BitmapPointer currentPointer = default;
    public readonly int NextBit() => MinBit + Length;

    public void Increment() => currentPointer.Increment();
}

public struct BitmapTemplate
{
    private readonly ContextBitRun[] runs;
    private readonly int mask;
    public readonly int BitsRequired() => runs[0].NextBit();
    public ushort contextValue = 0;
    
    
    public BitmapTemplate(ContextBitRun[] runs)
    {
        this.runs = runs;
        mask = ComputeMask(runs);
    }
    
    private static int ComputeMask(Span<ContextBitRun> localOffsets)
    {
        // mask starts out as 0b011111111111...
        // we shift zeros in from the left, and set the bottom bit of each run to one
        // then we invert the mask so we get ones at all the places we want to preserv
        // and zeros above the bitrun and on the last bit of each run.
        var ret = int.MaxValue;
        foreach (var bitRun in localOffsets)
        {
            ret <<= bitRun.Length;
            ret |= 1;
        }
        return ~ret;
    }
    
    
    public readonly ushort OldReadContext(IBinaryBitmap bitmap, int row, int col)
    {
        ushort ret = 0;
        for(int j = 0; j < runs.Length; j++)
        {
            ref var run = ref runs[j];
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
    
    public void ResetContext(IBinaryBitmap bitmap, int row, int col)
    {
        contextValue = 0;
        for(int j = 0; j < runs.Length; j++)
        {
            ref var run = ref runs[j];
            var runPtr = bitmap.PointerFor(row + run.Y, col + run.X); 
            for (int i = 0; i < run.Length; i++)
            {
                contextValue <<= 1;
                contextValue |= runPtr.CurrentValue;
                runPtr.Increment();
            }
            run.currentPointer = runPtr;
        }
    }
    
    public void Increment()
    {
        var newBits = CollectNewBits();
        contextValue = (ushort)(((contextValue << 1) & mask) | newBits);
    }

    private int CollectNewBits()
    {
        var newBits = 0;
        foreach (var run in runs)
        {
            newBits <<= run.Length;
            newBits |= run.currentPointer.CurrentValue;
            run.Increment();
        }
        return newBits;
    }

    
    
}