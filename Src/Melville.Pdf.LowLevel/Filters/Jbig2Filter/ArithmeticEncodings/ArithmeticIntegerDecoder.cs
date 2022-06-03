using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public class ArithmeticIntegerDecoder: EncodedReader<ContextStateDict, MQDecoder>
{
    private readonly ArithmeticBitmapReaderContext template;

    public ArithmeticIntegerDecoder(ArithmeticBitmapReaderContext template) : base(new MQDecoder())
    {
        this.template = template;
    }

    public override bool IsOutOfBand(int item) => item == int.MaxValue;

    private static readonly (int BitsToRead, int Offset)[] encodingModes =
    {
        (2, 0), (4, 4), (6, 20), (8, 84), (12, 340), (32, 4436)
    };
    
    protected override int Read(ref SequenceReader<byte> source, ContextStateDict context)
    {
        var prev = new AritmeticIntegerContext(context);
        var sign = GetBit(ref source, ref prev);
        var bitLen = PickBitLength(ref source, ref prev);
        return AssembleNumber(sign, 
            ReadInt(ref source, ref prev, bitLen));
    }

    private int ReadInt(
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
    private int PickBitLength(ref SequenceReader<byte> reader, ref AritmeticIntegerContext context)
    {
        int i = 0;
        while ((i < (encodingModes.Length - 1)) &&
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