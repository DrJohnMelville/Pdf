using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Melville.Parsing.StreamFilters.AsciiHex;
public class AsciiHexDecoder: IStreamFilterDefinition
{
#warning make this a static singleton
    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output) =>
        InnerConvert(ref input, ref output, false);

    public static bool InnerConvert(ref SpanParsingBuffer input, ref Span<byte> output, bool done)
    {
        var peek = input.Peek();

        var terminatingIndex = peek.IndexOf((byte)'>');
        if (terminatingIndex >= 0)
        {
            done = true;
            peek = peek[..terminatingIndex];
        }
        while (output.Length > 0)
        {

            var index1 = peek.IndexOfAny(hexChars);
            if (index1 < 0) return done;
            var index2 = peek[(index1 + 1)..].IndexOfAny(hexChars);
            if (index2 < 0)
            {
                if (!done) return false;
                PushByte(ref output, peek[index1], (byte)'0');
                input.Advance(index1 + 1);
                return true;
            }
            index2 += index1 + 1;
            PushByte(ref output, peek[index1], peek[index2]);
            input.Advance(index2 + 1);
            peek = peek[(index2 + 1)..];
        }
        return false;
    }

    private static readonly SearchValues<byte> hexChars = SearchValues.Create("0123456789abcdefABCDEF"U8);

    private static void PushByte(ref Span<byte> output, byte b1, byte b2)
    {
        output.SetAndIncrement(ByteFromHexChars(b1, b2));
    }

    private static byte ValueFromDigit(byte digitChar) => digitChar switch
    {
        >= (byte)'0' and <= (byte)'9' => (byte)(digitChar - '0'),
        >= (byte)'A' and <= (byte)'Z' => (byte)(digitChar - 'A' + 10),
        >= (byte)'a' and <= (byte)'z' => (byte)(digitChar - 'a' + 10),
        _ => byte.MaxValue
    };

    private static byte ByteFromHexChars(byte char1, byte char2) =>
        (byte)((ValueFromDigit(char1) << 4) | ValueFromDigit(char2));


    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) =>
      InnerConvert(ref input, ref output, true);

    public int MinWriteSize => 1;

}

public static class PushByteImplementation
{
    extension<T>(ref Span<T> self)
    {
        public void SetAndIncrement(T element)
        {
            self[0] = element;
            self = self[1..];
        }

        public bool TrySetAndIncrement(T element)
        {
            if (self.Length is 0) return false;
            self.SetAndIncrement(element);
            return true;
        }
    }
}