using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Melville.Pdf.DataModelTests.Writer.CcittParts;

public static class StringToByteArray
{
    public static SequenceReader<byte> CreateSequence(string bits)
    {
        var seq = CreateArray(bits);
        return new SequenceReader<byte>(new ReadOnlySequence<byte>(seq));
    }

    public static byte[] CreateArray(string bits)
    {
        var byteBits = bits.Where(i => i is '1' or '0').Select(i => i == '1').ToArray();
        var seq = byteBits
            .Concat(PadToByte(byteBits.Length))
            .Chunk(8)
            .Select(BitsToBytes)
            .ToArray();
        return seq;
    }

    private static byte BitsToBytes(bool[] arg) =>
        arg.Aggregate<bool, byte>(0, (prior, isone) => (byte)((prior << 1) | (isone ? 1 : 0)));

    private static IEnumerable<bool> PadToByte(int length)
    {
        var extra = length % 8;
        return extra == 0 ? Array.Empty<bool>() : Enumerable.Repeat(false, 8 - extra);
    }
}