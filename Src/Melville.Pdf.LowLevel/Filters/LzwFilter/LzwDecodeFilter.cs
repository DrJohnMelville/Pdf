using CoreJ2K.Util;
using Melville.Parsing.StreamFilters;
using Melville.Parsing.VariableBitEncoding;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;


internal class LzwDecodeFilter : IStreamFilterDefinition
{
    private readonly BitReader reader;
    private readonly DecoderDictionary dictionary = new();
    private readonly BitLength bits;
    private const short EmptyCode = -1;

    public LzwDecodeFilter(int sizeSwitchFlavorDelta)
    {
        bits = new BitLength(9, sizeSwitchFlavorDelta);
        reader = new BitReader();
    }

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output) 
    {
        while (true)
        {
            TryWriteCurrentCode(ref output);
            if (output.IsEmpty) return false;

            switch (reader.TryRead(bits.Length, ref input))
            {
                case null: return false;
                case LzwConstants.EndOfFileCode: return true;
                case LzwConstants.ClearDictionaryCode:
                    ResetDictionary();
                    break;
                case var item:
                    HandleCodedGroup((short)item);
                    break;
            }
        }
    }

    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        Convert(ref input, ref output) || OutOfData(input.CurrentLength);

    bool OutOfData(int inputlength) => NoCodeToWrite() && inputlength == 0;
    // Notice that BitReader takes source bits one byte at a time.  All LZW code words are at least 9
    // bits, so every code read must read at least one byte from the input stream.  Thus if there are
    // no more characters in the input stream, then there is not an entire code waiting in the bit
    // reader.

    private void ResetDictionary()
    {
        dictionary.Reset();
        bits.SetBitLength(9);
        codeBeingWritten = EmptyCode;
    }

    private void HandleCodedGroup(short item)
    {
        if (dictionary.IsDefined(item))
        {
            HandleKnownCode(item);
        }
        else
        {
            HandleUnknownCode();
        }
    }

    private void HandleUnknownCode()
    {
        var child = dictionary.AddChild(codeBeingWritten, dictionary.FirstChar(codeBeingWritten));
        CheckBitLength(child);
        ScheduleNewCodeForOutput(
            child);
    }

    private void CheckBitLength(short child)
    {
        // the decoder is always one code behind the encoder, so we add 1 to the switching logic.
        bits.CheckBitLength(child + 1);
    }

    private void HandleKnownCode(short item)
    {
        if (codeBeingWritten >= 0)
        {
            CheckBitLength(dictionary.AddChild(codeBeingWritten, dictionary.FirstChar(item)));
        }

        ScheduleNewCodeForOutput(item);
    }

    private short codeBeingWritten = EmptyCode;
    private int lastWrittenIndex = int.MaxValue;

    private void ScheduleNewCodeForOutput(short code)
    {
        codeBeingWritten = code;
        lastWrittenIndex = -1;
    }
    private void TryWriteCurrentCode(ref Span<byte> destination)
    {
        if (NoCodeToWrite()) return;
        lastWrittenIndex = dictionary.WriteChars(codeBeingWritten, ref destination, lastWrittenIndex);
    }

    private bool NoCodeToWrite()
    {
        return codeBeingWritten == EmptyCode || codeBeingWritten == lastWrittenIndex;
    }
}