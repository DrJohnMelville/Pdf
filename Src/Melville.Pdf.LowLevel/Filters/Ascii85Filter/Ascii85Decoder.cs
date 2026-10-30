using Melville.Parsing.StreamFilters;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter;

internal class Ascii85Decoder2 : StringDecoderStreamFilterDefinition<Ascii85StringDecoder, byte>
{
    public override int MinWriteSize => 4;

    protected override ReadOnlySpan<byte> TerminatorSequence() => "~>"u8;

}

internal class Ascii85Decoder : IStreamFilterDefinition
{
    private uint buffer = 0;
    private int bytesRead = 0;
    private int bytesToWrite = 0;

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        InnerConvert(ref input, ref output, false);

    public bool InnerConvert(ref SpanParsingBuffer input, ref Span<byte> output, bool final)
    {
        while (true)
        {
            if (bytesToWrite > 0)
            {
                if (!TryWriteBuffer(ref output)) return false;
            }
            else
            {
                if (!TryReadCode(ref input))
                {
                    if (!final) return false;
                    if (bytesRead is 0) return true;
                    PushPartialByte();
                }
            }

        }
    }
    void PushPartialByte()
    {
        Debug.Assert(bytesRead > 1);
        bytesToWrite = bytesRead - 1;
        for (; bytesRead < 5; bytesRead++) PushByte(Ascii85Constants.IncompleteGroupPadding);
        for (int i = 3; i >= bytesToWrite; i--) buffer >>= 8;
        bytesRead = 0;
    }
    bool TryReadCode(ref SpanParsingBuffer input)
    {
        Debug.Assert(bytesToWrite == 0);
        for (; bytesRead < 5; bytesRead++)
        {
            if (!input.TryAdvanceToExcept(CharacterClassifier.WhitespaceSearchValues)) return false;
            var b = input.Peek()[0];
            if (b == Ascii85Constants.FirstTerminatingChar) return false;
            input.Advance(1);
            if (b == Ascii85Constants.FourZeroChar)
                ExpandAsZeros();
            else
                PushByte(b);
        }
        Debug.Assert(bytesRead == 5);
        bytesToWrite = 4;
        bytesRead = 0;
        return true;
    }
    void ExpandAsZeros()
    {
        Debug.Assert(buffer is 0);
        Debug.Assert(bytesRead is 0);
        buffer = 0;
        bytesRead = 4;
    }
    void PushByte(byte b)
    {
        Debug.Assert((uint)bytesRead < 5);
        buffer *= 85u;
        buffer += (uint)(b - Ascii85Constants.FirstChar);
    }
    bool TryWriteBuffer(ref Span<byte> output)
    {
        Debug.Assert(bytesRead == 0);
        while (bytesToWrite > 0)
        {
            if (output.Length is 0) return false;
            bytesToWrite--;
            output[0] = (byte)(buffer >> (8*bytesToWrite) );
            output = output[1..];
        }
        buffer = 0;
        bytesRead = 0;
        return true;
    }


    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        InnerConvert(ref input, ref output, true);

    /// <summary>
    /// Minimum size of a buffer that can be written to.
    /// </summary>
    public int MinWriteSize => 0;
}
