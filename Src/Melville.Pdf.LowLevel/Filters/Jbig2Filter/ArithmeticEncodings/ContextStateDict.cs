using System.Diagnostics;
using System.Security.Authentication.ExtendedProtection;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.HalftoneRegionParsers;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

[DebuggerDisplay("I: {I}  MPS: {MPS}")]
public struct ContextEntry
{
    public ushort I;
    public byte MPS;
}


public readonly struct ContextStateDict
{
    private readonly ContextEntry[] entries;
    public int ContextEntryCount => entries.Length;

    public ContextStateDict(int bitsInContextTemplate)
    {
        entries = new ContextEntry[1 << bitsInContextTemplate];
    }

    public ref ContextEntry EntryForContext(ushort context) => ref entries[context];
    
}