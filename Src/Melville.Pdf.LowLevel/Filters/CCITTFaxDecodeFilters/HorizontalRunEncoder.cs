using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class HorizontalRunEncoder
{
    private readonly (byte, uint)[] terminatingSpans;
    private readonly (byte, uint)[] makeupSpans;

    public HorizontalRunEncoder((byte, uint)[] terminatingSpans, (byte, uint)[] makeupSpans)
    {
        Debug.Assert(terminatingSpans.Length == 64);
        this.terminatingSpans = terminatingSpans;
        this.makeupSpans = makeupSpans;
    }

    public bool WriteRun(ref BitTarget target, int length)
    {
        Debug.Assert(length >= 0);
        return length < 64 ? 
            DoWrite(ref target, terminatingSpans[length]) : 
            WriteWithMakeupRun(ref target, length);
    }

    private bool WriteWithMakeupRun(ref BitTarget target, int length)
    {
        var makeupRun = Math.Min(SelectMakeupSpan(length), makeupSpans.Length);
        return DoWrite(ref target, makeupSpans[makeupRun - 1]) &&
               WriteRun(ref target, length - (64 * makeupRun));
    }

    private static int SelectMakeupSpan(int length) => length / 64; 
    // Integer division discards the remainder, so this will find the biggest span smaller than length.

    private bool DoWrite(ref BitTarget target, (byte Length, uint Data) span) =>
        target.TryWriteBits(span.Data, span.Length);
}