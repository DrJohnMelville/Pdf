using System.Buffers;
using System.Diagnostics;

namespace Melville.JBig2.ArithmeticEncodings;


internal struct TypicalIntegerDecoder 
{
    private int value;
    private readonly ContextStateDict dict;
    public MQDecoder Decoder { get; }

    public TypicalIntegerDecoder(ContextStateDict dict, MQDecoder decoder)
    {
        this.dict = dict;
        Decoder = decoder;
        value = 1;
    }
    
    private static readonly (int BitsToRead, int Offset)[] encodingModes =
    {
        (2, 0), (4, 4), (6, 20), (8, 84), (12, 340), (32, 4436)
    };

    public int Read(ref SequenceReader<byte> source)
    {
        var sign = GetBit(ref source);
        var bitLen = PickBitLength(ref source);
        return AssembleNumber(sign, ReadInt(ref source, bitLen));
    }

    private int ReadInt(
        ref SequenceReader<byte> source, int encodingMode)
    {
        var (bits, offset) = encodingModes[encodingMode];
        int value = 0;
        for (int i = 0; i < bits; i++)
        {
            value <<= 1;
            value |= GetBit(ref source);
        }
        return value + offset;
    }

    private int PickBitLength(ref SequenceReader<byte> reader)
    {
        int i = 0;
        while ((i < (encodingModes.Length - 1)) &&
               !LengthSelectionBitFound(ref reader)) i++;
        return i;
    }

    private bool LengthSelectionBitFound(ref SequenceReader<byte> source) =>
        GetBit(ref source) == 0;

    private int AssembleNumber(int sign, int magnitude) =>
        (sign, magnitude) switch
        {
            (1, 0) => int.MaxValue,
            (1, _) => -magnitude,
            _ => magnitude
        };

    private int GetBit(ref SequenceReader<byte> source)
    {
        var bit = Decoder.GetBit(ref source, ref GetContext());
        UpdateContext(bit);
        return bit;
    }
    
    private void UpdateContext(int bit)
    {
        Debug.Assert(bit is 0 or 1);
        value = (int)(value < 256 ? ShiftBitIntoPrev(bit) : RemoveTopBit(ShiftBitIntoPrev(bit)));
    }
    private int RemoveTopBit(int shiftedPrev) => (shiftedPrev & 511) | 256;
    private int ShiftBitIntoPrev(int bit) => (value << 1) | bit;

    private ref ContextEntry GetContext() => ref dict.EntryForContext(value);
}