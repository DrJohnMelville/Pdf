using System;
using System.Buffers;
using System.Diagnostics;
using System.Xml;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

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

public class ArithmeticIntegerDecoder: EncodedReader<ContextStateDict, MQDecoder>
{
    private readonly ArithmeticBitmapReaderContext template;

    public ArithmeticIntegerDecoder(ArithmeticBitmapReaderContext template) : base(new MQDecoder())
    {
        this.template = template;
    }

    public override bool IsOutOfBand(int item) => item == int.MaxValue;

    private static int[] bitsToRead = { 2, 4, 6, 8, 12, 32 };
    private static int[] numberOffset = { 0,4,20,84, 340, 4436};
    
    protected override int Read(ref SequenceReader<byte> source, ContextStateDict context)
    {
        var prev = new AritmeticIntegerContext(context);
        var sign = GetBit(ref source, ref prev);
        var bitLen = PickBitLength(ref source, ref prev);
        return AssembleNumber(sign, 
            ReadInt(ref source, ref prev, bitsToRead[bitLen], numberOffset[bitLen]));
    }

    private int ReadInt(
        ref SequenceReader<byte> source, ref AritmeticIntegerContext prev, int bits, int offset)
    {
        int value = 0;
        for (int i = 0; i < bits; i++)
        {
            value <<= 1;
            value |= GetBit(ref source, ref prev);
        }
        return value + offset;
    }
    private int PickBitLength(ref SequenceReader<byte> reader, ref AritmeticIntegerContext context)
    {
        int i = 0;
        while ((i < (bitsToRead.Length - 1)) &&
             !LengthSelectionBitFound(ref reader, ref context)) i++;
        return i;
    }
    
    private bool LengthSelectionBitFound(
        ref SequenceReader<byte> source, ref AritmeticIntegerContext prev) => 
        GetBit(ref source, ref prev) == 0;

    private int AssembleNumber(int sign, int magnitude) =>
        (sign, magnitude) switch
        {
            (1, 0) => int.MaxValue,
            (1, _) => -magnitude,
            _ => magnitude
        };

    private int GetBit(
        ref SequenceReader<byte> source, ref AritmeticIntegerContext prev)
    {
        var bit = State.GetBit(ref source, ref prev.GetContext());
        prev.UpdateContext(bit);
        return bit;
    }

    public override void ReadBitmap(ref SequenceReader<byte> source, BinaryBitmap target) => 
        target.ReadArithmeticEncodedBitmap(ref source, State, template, false, false);
}