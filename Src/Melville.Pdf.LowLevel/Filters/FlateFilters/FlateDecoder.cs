using Melville.Parsing.StreamFilters;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Diagnostics;
using System.IO.Compression;
using static Melville.Pdf.LowLevel.Filters.FlateFilters.FlateDecoderState;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters;

internal enum FlateDecoderState
{
    Prefix = 0,
    Decoding = 1,
    Closed = 2
}

internal class FlateDecoder : IStreamFilterDefinition, IDisposable
{
    private readonly DeflateDecoder decoder = new DeflateDecoder();
    FlateDecoderState state;

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        switch (state)
        {
            case Prefix:
                if (input.CurrentLength < 2) return false;
                input.Advance(2);
                state = Decoding;
                goto case Decoding;
            case Decoding:
                DecodeLoop(ref input, ref output);
                return state is Closed;
            default: return true;
        }
    }

    private void DecodeLoop(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        if (decoder.Decompress(input.Peek(), output, out var consumed, out var written) is
            System.Buffers.OperationStatus.Done)
        {
            state = Closed;
            decoder.Dispose();
        }
        input.Advance(consumed);
        output = output[written..];
    }

    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) => Convert(ref input, ref output);

    public void Dispose()
    {
        if (state is not Closed) decoder.Dispose();
        state = Closed;
    }
}