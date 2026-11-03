using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using static Melville.Pdf.LowLevel.Filters.FlateFilters.FlateEncoderState;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters;

internal class FlateCodecDefinition : ICodecDefinition
{
    public  ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters, object? context) =>
        ValueTask.FromResult(ReadingFilterStream.Wrap(data, new FlateEncoder()));

    public  ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters, object? context) => ValueTask.FromResult(ReadingFilterStream.Wrap(input, new FlateDecoder()));
}

internal enum FlateEncoderState
{
    WritePrefix,
    CopyBytes,
    WriteAdler,
    Closed
}

internal sealed class FlateEncoder : IStreamFilterDefinition, IDisposable
{
    private readonly DeflateEncoder encoder = new();
    private readonly Adler32Computer adler = new();
    private int byteCount = 0;
    private FlateEncoderState state;

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output) =>
            Convert(ref input, ref output, false);
    bool Convert(ref SpanParsingBuffer input, ref Span<byte> output, bool final)
    {
        switch (state)
        {
            case WritePrefix:
                if (WriteBuffer(PrefixBytes(), ref output, CopyBytes))
                    goto case CopyBytes;
                break;
            case CopyBytes:
                if (!WriteBytes(ref input, ref output, final)) break;
                state = WriteAdler;
                byteCount = 0;
                goto case WriteAdler;
            case WriteAdler:
                if (WriteBuffer(WriteAdlerHash(stackalloc byte[8]), ref output, Closed))
                {
                    encoder.Dispose();
                    return true;
                }
                break;
            default: return true;
        }
        return false;
    }

    Span<byte> WriteAdlerHash(Span<byte> target)
    {
        BinaryPrimitives.WriteUInt64BigEndian(target, adler.GetHash());
        return target;
    }

    bool WriteBytes(ref SpanParsingBuffer input, ref Span<byte> output, bool final)
    {
        var ret = encoder.Compress(input.Peek(), output, out var inputConsumed, out var outputWritten, final);
        adler.AddData(input.Peek(inputConsumed));
        input.Advance(inputConsumed);
        output = output[outputWritten..];
        return final && ret == System.Buffers.OperationStatus.Done;
    }

    private static ReadOnlySpan<byte> PrefixBytes() => new byte[] { 0x78, 0xDA };

    public bool WriteBuffer(scoped ReadOnlySpan<byte> buffer, ref Span<byte> target, FlateEncoderState nextState)
    {
        buffer = buffer[byteCount..];
        var source = buffer.TakeMax(target.Length, out var actualLength);
        source.CopyTo(target);
        target = target[actualLength..];
        byteCount += actualLength;
        if (actualLength == buffer.Length)
        {
            state = nextState;
            return true;
        }
        return false;
    }
    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) => Convert(ref input, ref output, true);

    public void Dispose()
    {
        if (state is not Closed) encoder.Dispose();
        state = Closed;
    }

}