
namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public struct ContextEntry
{
    public ushort I;
    public byte MPS;
}

public readonly struct ContextStateDict
{
    private readonly ContextEntry[] entries;

    public ContextStateDict(int bitsInContextTemplate)
    {
        entries = new ContextEntry[1 << bitsInContextTemplate];
    }

    public ref ContextEntry EntryForContext(ushort context) => ref entries[context];
}