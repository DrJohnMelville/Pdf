using Melville.Parsing.StreamFilters;
using Melville.Parsing.VariableBitEncoding;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

internal class LzwEncodeFilter : IStreamFilterDefinition
{
    private readonly BitWriter output = new();
    private readonly EncoderDictionary dictionary = new();
    private short currentDictionaryEntry = -1;
    private readonly BitLength bits;
    private readonly byte[] outputBufferBacker = new byte[5];
    private Memory<byte> outputBuffer = Array.Empty<byte>();

    public LzwEncodeFilter(int earlySwitchLength)
    {
        bits = new BitLength(9, earlySwitchLength);
    }

    public int MinWriteSize => 10;

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        if (currentDictionaryEntry < 0)
        {
            if (!input.TryGetUInt8(out var firstByte)) return false;
            currentDictionaryEntry = firstByte;
        }

        while (TryWriteBytes(ref output) && input.TryGetUInt8(out var nextByte))
        {
            if (dictionary.GetOrCreateNode(currentDictionaryEntry, nextByte, out var nextEntry))
            {
                currentDictionaryEntry = nextEntry;
            }
            else
            {
                Debug.Assert(outputBuffer.IsEmpty);
                var written = WriteCodeForCurrentChain(outputBufferBacker, nextEntry, nextByte);
                outputBuffer = outputBufferBacker.AsMemory(0, written);
            }
        }

        return false;
    }
    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) => Convert(ref input, ref output);

    bool TryWriteBytes(ref Span<byte> output)
    {
        if (output.IsEmpty) return false;
        if (outputBuffer.IsEmpty) return true;
        var source = outputBuffer.Span.TakeMax(output.Length, out var actualLength);
        source.CopyTo(output);
        output = output[actualLength..];
        outputBuffer = outputBuffer[actualLength..];
        return true;
    }

    private int WriteCodeForCurrentChain(Span<byte> destination, short nextEntry, byte nextByte)
    {
        var len = output.WriteBits(currentDictionaryEntry, bits.Length,
            destination);
        len += UpdateEncodingState(nextEntry, destination, len);
        currentDictionaryEntry = nextByte;
        return len;
    }

    private int UpdateEncodingState(short nextEntry, in Span<byte> destination, int destPos) =>
        HitMaximumDictionarySize(nextEntry) ? ResetDictionary(destination, destPos) : CheckBitLength(nextEntry);

    private int CheckBitLength(short nextEntry)
    {
        bits.CheckBitLength(nextEntry);
        return 0;
    }

    private int ResetDictionary(Span<byte> destination, int destPos)
    {
        var len = output.WriteBits(LzwConstants.ClearDictionaryCode, bits.Length,
            destination[destPos..]);
        dictionary.Reset();
        bits.SetBitLength(9);
        return len;
    }

    private static bool HitMaximumDictionarySize(short nextEntry) =>
        nextEntry >= LzwConstants.MaxTableSize - 2;


    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
        ref SequenceReader<byte> source,
        in Span<byte> destination)
    {
        if (!HasRoomToWrite(destination, 2)) return (source.Position, 0, false);
        var len = output.WriteBits(currentDictionaryEntry, bits.Length, destination);
        len += output.WriteBits(LzwConstants.EndOfFileCode, bits.Length, destination[len..]);
        len += output.FinishWrite(destination[len..]);
        return (source.Position, len, true);
    }

    private static bool HasRoomToWrite(Span<byte> destination, int destPosition)
    {
        return destPosition + 4 < destination.Length;
    }
}