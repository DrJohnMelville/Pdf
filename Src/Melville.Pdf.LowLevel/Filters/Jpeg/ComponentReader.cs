using System;
using System.Buffers.Text;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public readonly partial struct ComponentDefinition
{
    [FromConstructor] public ComponentId Id { get; }
    [FromConstructor] private readonly int samplingFactors;
    [FromConstructor] public int QuantTableNumber { get; }
    public int HorizontalSamplingFactor => samplingFactors & 0b1111;
    public int VerticalSamplingFactor => (samplingFactors>>4) & 0b1111;

}

public readonly partial struct ComponentReader
{
    [FromConstructor] private readonly ComponentDefinition definition;
    [FromConstructor] private readonly HuffmanTable acHuffman;
    [FromConstructor] private readonly HuffmanTable dcHuffman;

    private readonly int[] mcuValues = new int[64];
    
    public async ValueTask ReadMcuAsync(AsyncBitSource source)
    {
        await ReadDcAsync(source).CA();
        await ReadAcValuesAsync(source).CA();
    }

    private async ValueTask ReadDcAsync(AsyncBitSource source)
    {
        var dcBitLen = await dcHuffman.ReadAsync(source).CA();
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
            var (zeroCount, bitsToRead) =  
                ((uint)await acHuffman.ReadAsync(source).CA())
                .SplitHighAndLowBits(4);
            if (TryHandleEndOfBlock(zeroCount, bitsToRead, pos)) return;
            
            var valueBits = await source.ReadBitsAsync((int)bitsToRead).CA();
            var finalValue = DecodeNumber((int)bitsToRead, (int)valueBits);
         
            pos = HandleAcPair((int)zeroCount, finalValue, pos);
        }
    }

    private bool TryHandleEndOfBlock(uint zeroCount, uint bitsToRead, int pos)
    {
        if (IsNotEndOfBlockCode(zeroCount, bitsToRead)) return false;
        HandleAcPair(63 - pos, 0, pos);
        return true;
    }

    private static bool IsNotEndOfBlockCode(uint zeroCount, uint bitsToRead)
    {
        return zeroCount != 0 || bitsToRead != 0;
    }

    private int HandleAcPair(int zeros, int finalValue, int pos)
    {
        for (int i = 0; i < zeros; i++)
        {
            PlaceMatrixValue(0, pos++);
        }
        PlaceMatrixValue(finalValue, pos++);
        return pos;
    }

    private int PlaceMatrixValue(int finalValue, int pos) => 
        mcuValues[ZizZagPositions.ZigZagToMatrix[pos]] = finalValue;
}