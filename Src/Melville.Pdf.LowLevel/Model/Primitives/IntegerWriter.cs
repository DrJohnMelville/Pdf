﻿using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.Primitives;

internal static class IntegerWriter
{
    public static ValueTask<FlushResult> WriteAndFlushAsync(PipeWriter target, long item)
    {
        var buffer = target.GetSpan(12);
        target.Advance(Write(buffer, item));
        return target.FlushAsync();
    }
    public static void Write(PipeWriter target, long item)
    {
        var buffer = target.GetSpan(22);
        target.Advance(Write(buffer, item));
    }

    public static int Write(in Span<byte> buffer, long item)
    {
        if (item < 0)
        {
            return WriteNegativeNumber(buffer, item);
        }
        uint value = (uint)item;
        return WritePositiveNumber(buffer, value, CountDigits(value));
    }

    private static int WriteNegativeNumber(in Span<byte> buffer, long item)
    {
        buffer[0] = (byte) '-';
        uint value = (uint) (-item);
        return WritePositiveNumber(buffer.Slice(1), value, CountDigits(value)) + 1;
    }

    public static void WriteFixedWidthPositiveNumber(in Span<byte> slice, long number, int width)
    {
        VerifyValidWriteRequest(number, width);
        WritePositiveNumber(slice, (ulong)number, width);
    }

    private static void VerifyValidWriteRequest(long number, int width)
    {
        if (number < 0 || CountDigits((ulong) number) > width)
            throw new ArgumentException("Cannot write fixed width positive number here.");
    }
        
    private static unsafe int WritePositiveNumber(in Span<byte> slice, ulong value, int countDigits)
    {
        var digits = countDigits;
        fixed (byte* current = slice)
        {
            byte* curPos = current + digits;
            do
            {
                value = DivRem(value, 10, out var remainder);
                *(--curPos) = (byte) ((byte) '0' + remainder);
            } while (curPos > current);
        }

        return digits;
    }
    internal static ulong DivRem(ulong a, ulong b, out ulong result)
    {
        ulong div = a / b;
        result = a - (div * b);
        return div;
    }

    public static int CountDigits(ulong value) => value switch
    {
        >9999999999999999999 => 20,
        >999999999999999999 => 19,
        >99999999999999999 => 18,
        >9999999999999999 => 17,
        >999999999999999 => 16,
        >99999999999999 => 15,
        >9999999999999 => 14,
        >999999999999 => 13,
        >99999999999 => 12,
        >9999999999 => 11,
        >999999999 => 10,
        >99999999 => 9,
        >9999999 => 8,
        >999999 => 7,
        >99999 => 6,
        >9999 => 5,
        >999 => 4,
        >99 => 3,
        >9 => 2,
        _ => 1
    };
}