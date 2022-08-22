using System;
using System.Buffers;
using System.Diagnostics;
using Melville.INPC;
using Melville.Parsing.SequenceReaders;
using SequenceReaderExtensions = Melville.Parsing.SequenceReaders.SequenceReaderExtensions;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class JpegStreamFactory
{
    [StaticSingleton]
    private partial class QuantizationTableParser:IJpegBlockParser
    {
        private const int QuantTableSize = 65;

        public void ParseBlock(SequenceReader<byte> data, JpegStreamFactory factory)
        {
            while (data.Remaining >= QuantTableSize)
            {
                int precision = data.ReadBigEndianUint8();
                int number = precision & 0xf;
                precision >>= 4;
                if (precision != 0)
                    throw new NotImplementedException("JPEG with precision other than 8 not implemented");
                var table = new int[64];
                for (int i = 0; i < table.Length; i++)
                {
                    table[i] = data.ReadBigEndianUint8();
                }
                Debug.WriteLine($"     Define QuantTable {number} {table[0]:X2} {table[1]:X2} .. {table[^2]:X2} {table[^1]:X2}");
                factory.quantizationTables[number] = new QuantizationTable(table);
            }
        }
    }
}

public readonly partial struct QuantizationTable
{
    // Per t.81 b.2.4.1 quantization table elements are stored in zigzag order
    [FromConstructor] private readonly int[] table;

    public int Dequantize(int zizZagIndex, int value) => table[zizZagIndex] * value;
}