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
        return state == Terminated;
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
        return state == Terminated;
    }
    bool ProcessOpcode(ref SpanParsingBuffer input, ref Span<byte> output) => state switch { 
            Opcode => ReadOpcode(ref input),
            Literal => CopyLiteral(ref input, ref output),
            Repeat => WriteRepeats(ref output),
            Terminated => false
        };
    bool ReadOpcode(ref SpanParsingBuffer input)
    {
        Debug.Assert(lengthRemaining == 0);
        if (!input.TryPeekUInt8(out var opcode)) return false;
        switch (opcode) 
        {
            case RleConstants.EndOfStream:
                state = Terminated;
                break;
            case < RleConstants.EndOfStream: // literal block
                lengthRemaining = opcode + 1;
                state = Literal;
                break;
            default: // repeat block
                if (input.Peek() is not [_, var repeat, ..]) return false;
                repeatedChar = repeat;
                lengthRemaining = RleConstants.RepeatedRunLength(opcode);
                state = Repeat;
                input.Advance(1);
                break;
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
