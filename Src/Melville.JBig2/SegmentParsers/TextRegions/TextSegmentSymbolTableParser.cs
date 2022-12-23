using System;
using System.Buffers;
using Melville.JBig2.HuffmanTables;
using Melville.Parsing.VariableBitEncoding;

namespace Melville.JBig2.SegmentParsers.TextRegions;

internal ref struct TextSegmentSymbolTableParser
{
    public static void Parse(ref SequenceReader<byte> reader, in Span<HuffmanLine> table)
    { 
        Span<int> runCodes = stackalloc int[35];
        for (int i = 0; i < 17; i++)
        {
            reader.TryRead(out var datum);
            runCodes[2 * i] = datum >> 4;
            runCodes[2 * i + 1] = datum & 7;
        }

        var bitContext = new BitReader();
        runCodes[34] = bitContext.ForceRead(4, ref reader);
        Span<HuffmanLine> runCodeTable = stackalloc HuffmanLine[35];
        HuffmanTableFactory.FromIntSpan(runCodes, runCodeTable);

        Span<int> symbolCodeLengths = stackalloc int[table.Length];
        var refCodeReader = new RunCodeInterpreter(reader, bitContext, runCodeTable);
        for (int i = 0; i < symbolCodeLengths.Length; i++)
        {
            symbolCodeLengths[i] = refCodeReader.GetNextCode();
        }

        reader = new SequenceReader<byte>(refCodeReader.UnexaminedSequence());
        
        HuffmanTableFactory.FromIntSpan(symbolCodeLengths, table);
    }
}