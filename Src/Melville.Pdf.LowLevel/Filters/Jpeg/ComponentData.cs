using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public readonly partial struct ComponentData
{
    [FromConstructor] public ComponentId Id { get; }
    [FromConstructor] private readonly int samplingFactors;
    [FromConstructor] public int QuantTableNumber { get; }
    private readonly int[] mcuValues = new int[64];

    public HuffmanTable AcHuffman { get; init; } = HuffmanTable.Empty;
    public HuffmanTable DcHuffman { get; init; } = HuffmanTable.Empty;

    public int HorizontalSamplingFactor => samplingFactors & 0b1111;
    public int VerticalSamplingFactor => (samplingFactors>>4) & 0b1111;

    public async ValueTask ReadMcuAsync(AsyncBitSource source)
    {
        await ReadDcAsync(source).CA();
        await ReadAvValuesAsync(source).CA();
    }

    private async ValueTask ReadDcAsync(AsyncBitSource source)
    {
        var dcBitLen = await DcHuffman.ReadAsync(source).CA();
        var dcBits = (int)await source.ReadBitsAsync(dcBitLen).CA();
        mcuValues[0] = DecodeNumber(dcBitLen, dcBits);
    }

    public static int DecodeNumber(int bitLen, int bits)
    {
        var l = BitUtilities.Exp2(bitLen - 1);
        return (int)(bits >= l ? bits : bits - (2 * l - 1));
    }

    private async ValueTask ReadAvValuesAsync(AsyncBitSource source)
    {
        int pos = 1;
        while (pos < 65)
        {
            var (zeroCount, bitsToRead) = await ReadAcAsync(source).CA();
            if (zeroCount == 0 && bitsToRead == 0) zeroCount = (uint)(65 - pos);
            for (int i = 0; i < zeroCount; i++)
            {
                mcuValues[pos++] = 0;
            }
            if (pos >= 65) break;
            var valueBits = await source.ReadBitsAsync((int)bitsToRead).CA();
            mcuValues[pos++] = DecodeNumber((int)bitsToRead, (int)valueBits);
        }
    }

    private async ValueTask<(uint LeadingZeros, uint AcNumber)> ReadAcAsync(AsyncBitSource source)
    {
        return ((uint)await AcHuffman.ReadAsync(source).CA()).SplitHighAndLowBits(4);
    }
}