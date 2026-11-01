using CoreJ2K.j2k.fileformat.metadata;
using CoreJ2K.Util;
using Melville.Parsing.StreamFilters;
using Melville.Parsing.StreamFilters.AsciiHex;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Buffers;
using System.Reflection.Emit;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;

internal class RunLengthEncoder: IStreamFilterDefinition
{
    byte state = RleConstants.EndOfStream;
    byte repeatingChatacter;

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        Convert(ref input, ref output, false);

    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        Convert(ref input, ref output, true);

    bool Convert(ref SpanParsingBuffer input, ref Span<byte> output, bool final)
    {
        while (output.Length > 0)
        {
            if (DoneReading(input, final)) return true;
            if (!WriteSingleCode(ref input, ref output, final)) return false;
        }
        return false;
    }

    private bool DoneReading(in SpanParsingBuffer input, bool final) => 
        state == RleConstants.EndOfStream && final && input.CurrentLength == 0;

    bool WriteSingleCode(ref SpanParsingBuffer input, ref Span<byte> output, bool final) => state switch
    {
        RleConstants.EndOfStream => PlanNextRun(ref input, ref output, final),
        > RleConstants.EndOfStream => WriteRepeats(ref output),
        _ => WriteLiteral(ref input, ref output)
    };
    bool PlanNextRun(ref SpanParsingBuffer input, ref Span<byte> output, bool final)
    {
        if (!final && input.CurrentLength < 4) return false;

        return IsTripple(input.Peek()) ?
            StartRepeatRun(ref input, ref output, final) :
            StartLiteralRun(ref input, ref output, ref final);
    }
    bool StartRepeatRun(ref SpanParsingBuffer input, ref Span<byte> output, bool final)
    {
        var peek = input.Peek();
        var length = peek.IndexOfAnyExcept(peek[0]);
        if (length < 0)
        {
            if (peek.Length <128 && !final) return false;
            length = peek.Length;
        }
        length = Math.Min(length, 128);
        state = (byte)(RleConstants.RepeatedRunLength(length));
        repeatingChatacter = peek[0];
        output.SetAndIncrement((byte) state);
        input.Advance(length);
        return true;
    }
    bool StartLiteralRun(ref SpanParsingBuffer input, ref Span<byte> output, ref bool final)
    {
        var peek = input.Peek();
        int length;
        for (length = 1; length < 128; length++)
        {
            if (length + 2 >= peek.Length )
            {
                length = peek.Length;
                if (final) break;
                return false;
            }
            if (IsTripple(peek[length..])) break;
        }
        state = (byte)(length -1);
        output.SetAndIncrement((byte)state);
        return true;
    }

    private static bool IsTripple(ReadOnlySpan<byte> peek) =>
        peek is [var a, var b, var c, ..] && a == b && a == c;

    bool WriteRepeats(ref Span<byte> output)
    {
        output.SetAndIncrement(repeatingChatacter);
        state = RleConstants.EndOfStream;
        return true;
    }
    bool WriteLiteral(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        var length = ((int)state) + 1;
        var target = output.TakeMax(Math.Min(input.CurrentLength,length), out var actualLength);
        input.Peek(actualLength).CopyTo(target);
        input.Advance(actualLength);
        output = output[actualLength..];
        var remainder = length - actualLength;
        state = remainder == 0 ? RleConstants.EndOfStream : (byte)(remainder - 1);
        return true;
    }




}