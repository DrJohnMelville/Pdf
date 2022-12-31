using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Model.Primitives;

internal static class WholeNumberParser
{
    /// <summary>
    /// Notice that this method reads one byte beyond the number and returns the last byte.
    /// We would have to rewind if we care about putting the last byte back.
    /// </summary>
    public static bool TryParsePositiveWholeNumber(
        ref SequenceReader<byte> input, out int value, out byte lastByteRead)
    {
        value = 0;
        while (true)
        {
            if (!input.TryRead(out lastByteRead)) return false;
            if (!IsDigit(lastByteRead)) return true;
            AddDgitToResult(ref value, lastByteRead);
        }
    }

    private static void AddDgitToResult(ref int value, byte lastByteRead)
    {
        value *= 10;
        value += CharToValue(lastByteRead);
    }

    /// <summary>
    /// Notice that this method reads one byte beyond the number and returns the last byte.
    /// We would have to rewind if we care about putting the last byte back.
    /// </summary>
    public static bool TryParsePositiveWholeNumber(
        ref SequenceReader<byte> input, out long value, out byte lastByteRead)
    {
        value = 0;
        while (true)
        {
            if (!input.TryRead(out lastByteRead)) return false;
            if (!IsDigit(lastByteRead)) return true;
            AddDgitToResult(ref value, lastByteRead);
        }
    }

    public static long ParsePositiveWholeNumber(in ReadOnlySpan<byte> input)
    {
        long ret = 0;
        foreach (var digit in input)
        {
            if (!IsDigit(digit))
                throw new InvalidOperationException("Trying to parse non digit");
            AddDgitToResult(ref ret, digit);
        }
        return ret;
    }

    private static void AddDgitToResult(ref long value, byte lastByteRead)
    {
        value *= 10;
        value += CharToValue(lastByteRead);
    }

    public static bool IsDigit(byte digitChar) => 
        digitChar is >= (byte) '0' and <= (byte) '9';

    public static int CharToValue(byte digitChar) => 
        digitChar - (byte) '0';
}