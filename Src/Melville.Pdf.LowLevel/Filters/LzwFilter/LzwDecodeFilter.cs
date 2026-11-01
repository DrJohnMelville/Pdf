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

    public int MinWriteSize => 1;

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
        Convert(ref input, ref output);

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
        if (codeBeingWritten == EmptyCode || codeBeingWritten == lastWrittenIndex) return;
        lastWrittenIndex = dictionary.WriteChars(codeBeingWritten, ref destination, lastWrittenIndex);    
    }

}