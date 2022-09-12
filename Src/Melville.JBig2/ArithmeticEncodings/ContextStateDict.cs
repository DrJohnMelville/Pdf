using System.Diagnostics;

namespace Melville.JBig2.ArithmeticEncodings;

[DebuggerDisplay("I: {I}  MPS: {MPS}")]
public struct ContextEntry
{
    public byte I;
    public byte MPS;

    public void InvertMPS()
    {
        Debug.Assert(MPS is 0 or 1);
        MPS =(byte) (1 - MPS);
    }
}


public readonly struct ContextStateDict
{
    private readonly ContextEntry[] entries;
    public int ContextEntryCount => entries.Length;

    public ContextStateDict(int bitsInContextTemplate)
    {
        entries = new ContextEntry[1 << bitsInContextTemplate];
    }

    public ref ContextEntry EntryForContext(int context) => ref entries[context];
    
}