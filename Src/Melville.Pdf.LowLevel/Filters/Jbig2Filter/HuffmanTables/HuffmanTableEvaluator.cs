using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public static class HuffmanTableEvaluator
{
    public static int ReadHuffmanInt(this ref SequenceReader<byte> reader, BitReader bitState, in ReadOnlySpan<HuffmanLine> lines)
    {
        var pattern = new HuffmanCode();
        foreach (var line in lines)
        {
            Debug.Assert(pattern.PrefixLength <= line.PrefixLengh, "Lines must be ascending length order");
            while (pattern.PrefixLength < line.PrefixLengh)
            {
                pattern = pattern.AddBitToPattern(ref reader, bitState);
            }
            
            if (line.Matches(pattern)) return line.ReadNum(ref reader, bitState);
        }

        throw new InvalidDataException("Got to the end of a huffman table");
    }

}