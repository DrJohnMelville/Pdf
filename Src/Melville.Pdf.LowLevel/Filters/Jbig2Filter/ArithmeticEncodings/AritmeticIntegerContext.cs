using System.Buffers;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;


public struct AritmeticIntegerContext 
{
    private ushort value;
    private readonly ContextStateDict dict;
    public MQDecoder Decoder { get; }

    public AritmeticIntegerContext(ContextStateDict dict, MQDecoder decoder)
    {
        this.dict = dict;
        Decoder = decoder;
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

public static class TypicalIngegerDecode
{
    private static readonly (int BitsToRead, int Offset)[] encodingModes =
    {
        (2, 0), (4, 4), (6, 20), (8, 84), (12, 340), (32, 4436)
    };

    public static int Read(ref SequenceReader<byte> source, AritmeticIntegerContext prev)
    {
        var sign = GetBit(ref source, ref prev);
        var bitLen = PickBitLength(ref source, ref prev);
        return AssembleNumber(sign, ReadInt(ref source, ref prev, bitLen));
    }

    private static int ReadInt(
        ref SequenceReader<byte> source, ref AritmeticIntegerContext prev, int encodingMode)
    {
        var (bits, offset) = encodingModes[encodingMode];
        int value = 0;
        for (int i = 0; i < bits; i++)
        {
            value <<= 1;
            value |= GetBit(ref source, ref prev);
        }
        return value + offset;
    }

    private static int PickBitLength(ref SequenceReader<byte> reader, ref AritmeticIntegerContext context)
    {
        int i = 0;
        while ((i < (encodingModes.Length - 1)) &&
               !LengthSelectionBitFound(ref reader, ref context)) i++;
        return i;
    }

    private static bool LengthSelectionBitFound(
        ref SequenceReader<byte> source, ref AritmeticIntegerContext prev) =>
        GetBit(ref source, ref prev) == 0;

    private static int AssembleNumber(int sign, int magnitude) =>
        (sign, magnitude) switch
        {
            (1, 0) => int.MaxValue,
            (1, _) => -magnitude,
            _ => magnitude
        };

    private static int GetBit(
        ref SequenceReader<byte> source, ref AritmeticIntegerContext prev)
    {
        var bit = prev.Decoder.GetBit(ref source, ref prev.GetContext());
        prev.UpdateContext(bit);
        return bit;
    }
}