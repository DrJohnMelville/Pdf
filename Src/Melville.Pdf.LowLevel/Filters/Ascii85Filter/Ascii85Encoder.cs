using CoreJ2K.j2k.image;
using Melville.Parsing.StreamFilters;
using Melville.Parsing.StreamFilters.AsciiHex;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter;

internal class Ascii85Encoder : IStreamFilterDefinition
{
    private uint code = 0;
    private int bytesToWrite = 0;

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        InnerConvert(ref input, ref output);
        return false;
    }

    private bool InnerConvert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        while (true)
        {
            if (!TryWrite(ref output)) return false;
            if (input.CurrentLength < 4) return true;
            code = input.GetUInt32BigEndian();
            bytesToWrite = 5;
        }
    }

    bool TryWrite(ref Span<byte> output)
    {
        if (bytesToWrite is 0) return true;
        if (output.Length is 0) return false;
        if (bytesToWrite is 5 && code is 0)
        {
            output.SetAndIncrement(Ascii85Constants.FourZeroChar);
            bytesToWrite = 0;
            return true;
        }
        WriteCompositeBytes(ref output);
        if ( bytesToWrite == 0)
        {
            code = 0;
            return true;
        }
        return false;
    }

    private void WriteCompositeBytes(ref Span<byte> output)
    {
        Span<byte> bytes = stackalloc byte[5];
        FillSpan(bytes);
        bytes = bytes[^bytesToWrite..];
        while (bytes is [var first, ..] && output.Length > 1)
        {
            output.SetAndIncrement(first);
            bytes = bytes[1..];
        }
        bytesToWrite = bytes.Length;
    }

    void FillSpan(Span<byte> bytes)
    {
        uint innerCode = code;
        for (int i = 4; i >= 0; i--)
        {
            var rem2 = innerCode % 85;
            (innerCode, var remainder) = Math.DivRem(innerCode, 85);
            bytes[i] = (byte)(remainder + Ascii85Constants.FirstChar);
        }
    }

    private int suffixState = 0;
    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        switch (suffixState)
        {
            case 0:
                if (!WritePartialCodes(ref input, ref output)) return false;
                suffixState = 1;
                goto case 1;
            case 1:
                if (!output.TrySetAndIncrement(Ascii85Constants.FirstTerminatingChar)) return false;
                suffixState = 2;
                goto case 2;
            case 2:
                if (!output.TrySetAndIncrement(Ascii85Constants.SecondTerminatingChar)) return false;
                suffixState = 3;
                return true;
            default:
                return true;
        }
    }

    private bool WritePartialCodes(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        if (!InnerConvert(ref input, ref output)) return false;
        var remaining = input.CurrentLength;
        Debug.Assert(remaining < 4);
        Debug.Assert(code is 0);
        if (remaining is 0) return true;
        for (int i = 0; i < 4; i++)
        {
            input.TryGetUInt8(out var b);
            code <<= 8;
            code |= b;
        }
        bytesToWrite = remaining + 1;
        for (int i = bytesToWrite; i < 5; i++) code /= 85;
        return TryWrite(ref output);
    }

}