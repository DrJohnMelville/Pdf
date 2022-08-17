using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Melville.INPC;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class JpegStreamFactory
{
    [StaticSingleton]
    public partial class HuffmanTableParser : IJpegBlockParser
    {
        public void ParseBlock(SequenceReader<byte> data, JpegStreamFactory factory)
        {
            int tableType = data.ReadBigEndianUint8();
            int tableNumber = tableType & 0xf;
            tableType >>= 4;
            Debug.WriteLine(    $"Table: {tableNumber} Type: {tableType}");
            ParseTable(ref data);
        }

        public HuffmanTable ParseTable(ref SequenceReader<byte> data)
        {
            Span<byte> codeCounts = stackalloc byte[16];
            data.TryCopyTo(codeCounts);
            data.Advance(16);
            var totalCodes = Sum(codeCounts);
        
            Span<byte> encoded = stackalloc byte[totalCodes];
            data.TryCopyTo(encoded);
            data.Advance(totalCodes);
            
            var lines = new HuffmanLine[totalCodes];
            DecodeHuffman(codeCounts, lines, encoded);

            return new HuffmanTable(lines);
        }

        private static void DecodeHuffman(Span<byte> codeCounts, HuffmanLine[] lines, Span<byte> encoded)
        {
            var nextCode = 0;
            var nextLine = 0;
            for (int i = 0; i < codeCounts.Length; i++)
            {
                for (int j = 0; j < codeCounts[i]; j++)
                {
                    lines[nextLine] = new HuffmanLine(i + 1, nextCode++, encoded[nextLine]);
                    nextLine++;
                }
                nextCode <<= 1;
            }
        }

        private int Sum(in Span<byte> codeCounts)
        {
            int ret = 0;
            foreach (var count in codeCounts)
            {
                ret += count;
            }
            return ret;
        }
    }
}

public readonly partial struct HuffmanTable
{
    [FromConstructor] private readonly HuffmanLine[] lines;

    public int Read(ref SequenceReader<byte> input, BitReader reader)
    {
        int key = 0;
        int bitlen = 0;
        foreach (var line in lines)
        {
            while (bitlen < line.BitLength)
            {
                bitlen++;
                key = (key << 1) | reader.ForceRead(1, ref input);
            }

            if (key == line.Code) return line.CodedValue;
        }
        throw new PdfParseException("Key does not exist in Huffman tab;e");
    }
}
public readonly partial struct HuffmanLine
{
    [FromConstructor] public int BitLength { get; }
    [FromConstructor] public int Code  { get; }
    [FromConstructor] public int CodedValue  { get; }
}
