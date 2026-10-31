using Melville.Parsing.StreamFilters;
using Melville.Parsing.StreamFilters.AsciiHex;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.AsciiHexFilters;

internal class AsciiHexEncoder : IStreamFilterDefinition
{
    public int MinWriteSize => 1;
    private byte nextByte = 0;

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        if (output is []) return false;
        if (nextByte is not 0)
        {
            output.SetAndIncrement(nextByte);
            nextByte = 0;
        }
        while (input.CurrentLength > 0 && output.Length > 1)
        {
            (output[0], output[1]) = HexMath.CharPairFromByte(input.GetUInt8());
            output = output[2..];
        }
        if (input.CurrentLength > 0 && output.Length == 1)
        {
            (output[0], nextByte) = HexMath.CharPairFromByte(input.GetUInt8());
            output = output[1..];
        }
        return false;
    }

    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        Convert(ref input, ref output);
        return (input.CurrentLength == 0 && nextByte == 0);

    }
}
