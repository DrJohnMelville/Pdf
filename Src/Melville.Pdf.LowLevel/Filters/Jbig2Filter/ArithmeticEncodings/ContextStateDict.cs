using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public struct ContextEntry
{
    public ushort I;
    public byte MPS;
}


public readonly struct ContextStateDict
{
    private readonly ContextEntry[] entries;
    public ushort Mask { get; }
    public bool ShouldUseTypicalAlgorithm => Mask == 0;

    public ContextStateDict(int bitsInContextTemplate, int iiadBitLength = 0)
    {
        entries = new ContextEntry[1 << bitsInContextTemplate];
        Mask = (ushort) ((1 << iiadBitLength) - 1);
    }

    public ref ContextEntry EntryForContext(ushort context) => ref entries[context];
    
}