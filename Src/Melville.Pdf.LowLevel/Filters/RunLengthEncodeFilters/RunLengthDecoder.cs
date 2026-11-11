using Melville.Parsing.StreamFilters;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Diagnostics;
using static Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters.RleDecoderState;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;


internal enum RleDecoderState
{
    Opcode = 0,
    Literal = 1,
    Repeat = 2,
    Terminated = 3
}

internal class RunLengthDecoder : IStreamFilterDefinition
{
    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        FinalConvert(ref input, ref output);
        return state == Terminated; // final convert might think it is done, but we know there may be more bytes comming.
    }

    private RleDecoderState state = Opcode;
    private int lengthRemaining;
    private byte repeatedChar;

    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        while (output.Length > 0)
        {
            if (!ProcessOpcode(ref input, ref output)) return state == Terminated;
        }
        return state == Terminated || DetectImplicitEndInsideFinalConvert(ref input);
    }

    private bool DetectImplicitEndInsideFinalConvert(ref SpanParsingBuffer input)
    {
        // if we are in final convert then no more bytes are comming.  If we are looking for an opcode,
        // and there are no more bytes, then we can report the stream as terminated.
        return (state == Opcode && input.CurrentLength == 0);
    }

    bool ProcessOpcode(ref SpanParsingBuffer input, ref Span<byte> output) => state switch { 
            Opcode => ReadOpcode(ref input),
            Literal => CopyLiteral(ref input, ref output),
            Repeat => WriteRepeats(ref output),
            _ => false
        };
    bool ReadOpcode(ref SpanParsingBuffer input)
    {
        Debug.Assert(lengthRemaining == 0);
        switch (input.Peek()) 
        {
            case [RleConstants.EndOfStream,..]:
                state = Terminated;
                break;
            case [< RleConstants.EndOfStream and var opcode,..]: // literal block
                lengthRemaining = opcode + 1;
                state = Literal;
                break;
            case [var opcode, var repeat, ..]: // repeat block
                repeatedChar = repeat;
                lengthRemaining = RleConstants.RepeatedRunLength(opcode);
                state = Repeat;
                input.Advance(1);
                break;
            default: return false;
        }
        input.Advance(1);
        return true;

    }
    bool CopyLiteral(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        var source = input.Peek().TakeMax(Math.Min(lengthRemaining, output.Length), out var length);
        input.Advance(length);
        source.CopyTo(output);
        return RecordWrite(ref output, length);
    }
    bool WriteRepeats(ref Span<byte> output)
    {
        var target = output.TakeMax(lengthRemaining, out var length);
        target.Fill(repeatedChar);
        return RecordWrite(ref output, length);
    }

    private bool RecordWrite(ref Span<byte> output, int length)
    {
        output = output[length..];
        lengthRemaining -= length;
        if (lengthRemaining == 0)
        {
            state = Opcode;
            return true;
        }
        return false;
    }
}
