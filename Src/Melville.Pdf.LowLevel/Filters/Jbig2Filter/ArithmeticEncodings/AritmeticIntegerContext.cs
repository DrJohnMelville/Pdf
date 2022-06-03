using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public ref struct AritmeticIntegerContext
{
    private ushort value;
    private readonly ContextStateDict dict;

    public AritmeticIntegerContext(ContextStateDict dict)
    {
        this.dict = dict;
        value = 1;
    }
    public void UpdateContext(int bit)
    {
        Debug.Assert(bit is 0 or 1);
        value = (ushort)(value < 256 ? ShiftBitIntoPrev(bit) : RemoveTopBit(ShiftBitIntoPrev(bit)));
    }
    private int RemoveTopBit(int shiftedPrev) => (shiftedPrev & 511) | 256;
    private int ShiftBitIntoPrev(int bit) => (value << 1) | bit;
    
    public ref ContextEntry GetContext()
    {
        return ref dict.EntryForContext(value);
    }
}