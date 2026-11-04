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
    private readonly BitWritingBuffer outputBuf = new();
    private readonly EncoderDictionary dictionary = new();
    private short currentDictionaryEntry = -1;
    private readonly BitLength bits;

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
                Debug.Assert(outputBuf.NothingToWrite);
                WriteCodeForCurrentChain(nextEntry, nextByte);
            }
        }

        return false;
    }
    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        Convert(ref input, ref output);
        if (input.CurrentLength > 0) return false;
        if (currentDictionaryEntry >= 0)
        {
            Debug.Assert(outputBuf.NothingToWrite);
            WriteBits(currentDictionaryEntry);
            WriteBits(LzwConstants.EndOfFileCode);
            currentDictionaryEntry = -1;
        }

        return TryWriteBytes(ref output);
    }

    void WriteBits(int code) => outputBuf.WriteBits(code, bits.Length);

    bool TryWriteBytes(ref Span<byte> output)
    {
        if (output.IsEmpty) return false;
        if (outputBuf.IsEmpty) return true;
        return outputBuf.TryWriteToMovingBuffer(ref output);
    }

    private void WriteCodeForCurrentChain(short nextEntry, byte nextByte)
    {
        WriteBits(currentDictionaryEntry);
        UpdateEncodingState(nextEntry);
        currentDictionaryEntry = nextByte;
    }

    private void UpdateEncodingState(short nextEntry)
    {
        if (HitMaximumDictionarySize(nextEntry))
            ResetDictionary();
        else
            CheckBitLength(nextEntry);
    }

    private void CheckBitLength(short nextEntry) => bits.CheckBitLength(nextEntry);

    private void ResetDictionary()
    {
        WriteBits(LzwConstants.ClearDictionaryCode);
        dictionary.Reset();
        bits.SetBitLength(9);
    }

    private static bool HitMaximumDictionarySize(short nextEntry) =>
        nextEntry >= LzwConstants.MaxTableSize - 2;

    private static bool HasRoomToWrite(Span<byte> destination, int destPosition)
    {
        return destPosition + 4 < destination.Length;
    }
}