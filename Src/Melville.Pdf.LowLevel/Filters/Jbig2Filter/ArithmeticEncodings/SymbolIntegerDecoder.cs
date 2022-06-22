using System.Buffers;
using System.Net.Sockets;
using System.Text;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public readonly ref struct SymbolIntegerDecoder
{
    private readonly MQDecoder decoder;
    private readonly ContextStateDict dict;
    
    public SymbolIntegerDecoder(MQDecoder decoder, ContextStateDict dict)
    {
        this.decoder = decoder;
        this.dict = dict;
    }

    public int Read(ref SequenceReader<byte> source)
    {
        var ret = 1;
        while (NeedMoreBitsToFillMask(ret)) ret = ShiftBitIntoNumber(ref source, ret);
        return RemoveLengthMarkerBit(ret);
    }

    private int ShiftBitIntoNumber(ref SequenceReader<byte> source, int ret) => 
        (ret << 1) | GetNextBit(ref source, ret);

    private bool NeedMoreBitsToFillMask(int ret) => ret < dict.ContextEntryCount;

    private int RemoveLengthMarkerBit(int ret) => ret - dict.ContextEntryCount;

    private int GetNextBit(ref SequenceReader<byte> source, int context) => 
        decoder.GetBit(ref source, ref dict.EntryForContext((int)context));
}