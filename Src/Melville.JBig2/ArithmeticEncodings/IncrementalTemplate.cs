using System;
using Melville.JBig2.BinaryBitmaps;

namespace Melville.JBig2.ArithmeticEncodings;

public ref partial struct IncrementalTemplate
{
    private readonly Span<ContextBitRun> runs;
    private readonly Span<BitmapPointer> pointers;
    private readonly int mask;
    public int context;

    public IncrementalTemplate(Span<ContextBitRun> runs) : this()
    {
        this.runs = runs;
        pointers = new BitmapPointer[runs.Length];
        context = 0;
        mask = ComputeMask(runs);
    }

    private static int ComputeMask(in Span<ContextBitRun> innerRuns)
    {
        var ret  = int.MaxValue;
        foreach (var run in innerRuns)
        {
            ret = (ret << run.Length) | 1;
        }
        return ~ret;
    }

    public void SetToPosition(IBinaryBitmap bmp, int row, int column) =>
        SetToPosition(bmp, bmp, row, column);
    public void SetToPosition(IBinaryBitmap bmp, IBinaryBitmap secondBitmap, int row, int column)
    {
        context = 0;
        for (int i = 0; i < runs.Length; i++)
        {
            ref var run = ref runs[i];
            ref var ptr = ref pointers[i];
            ptr = bmp.PointerFor(row + run.Y, column + run.X);
            for (int j = 0; j < run.Length; j++)
            {
                context = (context << 1) | ptr.CurrentValue;
                ptr.Increment();
            }

            if (run.IsLastRunInThisBitmap()) bmp = secondBitmap;
        }
    }

    public void Increment()
    {
        int newBits = 0;
        for (int i = 0; i < runs.Length; i++)
        {
            ref var ptr = ref pointers[i];
            newBits = (newBits << runs[i].Length) | ptr.CurrentValue;
            ptr.Increment();
        }
        context = ((context << 1) & mask) | newBits;
    }
}