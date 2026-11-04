using Melville.Parsing.StreamFilters;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.Predictors;

internal abstract class PngPredictingFilter: IStreamFilterDefinition
{
    protected PngPredictionBuffer Buffer;

    protected PngPredictingFilter(int colors, int bitsPerColor, int columns)
    {
        Buffer = new PngPredictionBuffer(colors, bitsPerColor, columns);
    }

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        while ((!output.IsEmpty) && (TryGetByte(ref input, out output[0])))
        {
            output = output[1..];
        }
        return false;
    }

    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        Convert(ref input, ref output);

    protected abstract bool TryGetByte(ref SpanParsingBuffer source, out byte byteToWrite);
}