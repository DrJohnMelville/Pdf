using System;
using System.Buffers.Text;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public readonly partial struct ComponentReader
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
        await ReadAcValuesAsync(source).CA();
    }

    private async ValueTask ReadDcAsync(AsyncBitSource source)
    {
        var dcBitLen = await DcHuffman.ReadAsync(source).CA();
        var dcBits = (int)await source.ReadBitsAsync(dcBitLen).CA();
        mcuValues[0] = DecodeNumber(dcBitLen, dcBits);
    }

    public static int DecodeNumber(int bitLen, int bits)
    {
        if (bitLen == 0) return 0;
        var baseValue = BitUtilities.Exp2(bitLen - 1);
        return (bits >= baseValue ? bits : 1+ bits - (2 *baseValue));
    }

    private async ValueTask ReadAcValuesAsync(AsyncBitSource source)
    {
        int pos = 1;
        while (pos < 64)
        {
            var (zeroCount, bitsToRead) = await ReadAcAsync(source).CA();
            if (zeroCount == 0 && bitsToRead == 0)
            {
                FillZeros(64 - pos, ref pos);
                return;
            }
            var valueBits = await source.ReadBitsAsync((int)bitsToRead).CA();
            var finalValue = DecodeNumber((int)bitsToRead, (int)valueBits);
            FillZeros((int)zeroCount, ref pos);
            mcuValues[ZizZagPositions.ZigZagToMatrix[pos++]] = finalValue;
        }
    }

    private void FillZeros(int zeroCount, ref int pos)
    {
        for (int i = 0; i < zeroCount; i++)
        {
            mcuValues[ZizZagPositions.ZigZagToMatrix[pos++]] = 0;
        }
    }

    private async ValueTask<(uint LeadingZeros, uint AcNumber)> ReadAcAsync(AsyncBitSource source)
    {
        var acCode = ((uint)await AcHuffman.ReadAsync(source).CA());
        return acCode.SplitHighAndLowBits(4);
    }
}