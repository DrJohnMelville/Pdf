using System;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Tokenze a readonlySpan into a number
/// </summary>
public static class NumberTokenizer
{
    /// <summary>
    /// Check if the span contains a number
    /// </summary>
    /// <param name="buffer">the characters to try and parse</param>
    /// <param name="value">the value if it parses as a number</param>
    /// <returns>True if successful, false otherwise</returns>
    public static bool TryDetectNumber(in ReadOnlySpan<byte> buffer, out PostscriptValue value) =>
        TryGetSignedDigitSequence(buffer, out var sign, out long longValue, out int length) switch
        {
            0 => PostscriptValueFactory.Create(SignValue(sign) * longValue).AsTrueValue(out value),
            (byte)'#' when (sign is 0 && longValue is >1 and <=36)  => 
                TryParseRadix((int)longValue, buffer[(length+1)..], out value),
            (byte)'.' => ParseAfterDecimal(sign, longValue, buffer[(length+1)..], out value),
            (byte)'e' or (byte)'E' =>
                ReadExponent(SignValue(sign)*longValue, buffer[(length+1)..], out value),
            _ => default(PostscriptValue).AsFalseValue(out value)
        };

    private static long SignValue(byte sign) => sign is (byte)'-' ? -1L : 1L;

    private static byte TryGetSignedDigitSequence(
        in ReadOnlySpan<byte> buffer, out byte sign, out long value, out int charsConsumed)
    {
        if (buffer[0] is ((byte)'-' or (byte)'+') and var signByte)
        {
            sign = signByte;
            return TryGetSubDigitSequence(1, buffer, out value, out charsConsumed);
        }

        sign = 0;
        return TryGetDigitSequence(10, buffer, out value, out charsConsumed);
    }

    private static byte TryGetSubDigitSequence(
        int prefix, in ReadOnlySpan<byte> buffer, out long value, out int charsConsumed)
    {
        var ret = TryGetDigitSequence(10, buffer[prefix..], out value, out charsConsumed);
        charsConsumed += prefix;
        return ret;
    }

    private static byte TryGetDigitSequence(
        int radix, in ReadOnlySpan<byte> buffer, out long value, out int charsConsumed)
    {
        value = 0;
        charsConsumed = 0;
        foreach (var digit in buffer)
        {
            var digitValue = CharacterClassifier.ValueFromDigit(digit);
            if (digitValue >= radix) return digit;
            value = (value*radix) + digitValue;
            charsConsumed++;
        }
        return 0;
    }

    private static bool TryParseRadix(
        int radix, ReadOnlySpan<byte> buffer, out PostscriptValue value) =>
        TryGetDigitSequence(radix, buffer, out var number, out var _) is 0?
            PostscriptValueFactory.Create(number).AsTrueValue(out value):
            default(PostscriptValue).AsFalseValue(out value);

    private static bool ParseAfterDecimal(
        byte sign, long longValue, ReadOnlySpan<byte> buffer, out PostscriptValue value)
    {
        return TryGetDigitSequence(10, buffer, out var digits, out var digitLength) switch
        {
            0 => PostscriptValueFactory.Create(
                    AddFractionToLong(sign, longValue, Fraction(digits, digitLength)))
                .AsTrueValue(out value),
            (byte)'e' or (byte)'E' =>
                ReadExponent(AddFractionToLong(sign, longValue, Fraction(digits, digitLength)),
                    buffer[(digitLength+1)..], out value),
            _ => default(PostscriptValue).AsFalseValue(out value)
        };
    }

    private static bool ReadExponent(
        double mantissa, ReadOnlySpan<byte> buffer, out PostscriptValue value) =>
        TryGetSignedDigitSequence(buffer, out var sign, out var digits, out var _) == 0
            ? PostscriptValueFactory.Create(mantissa * Math.Pow(10, SignValue(sign) * digits))
                .AsTrueValue(out value)
            : default(PostscriptValue).AsFalseValue(out value);

    private static double AddFractionToLong(byte sign, long longValue, double fraction) => 
        SignValue(sign)*(longValue+fraction);

    private static double Fraction(long digits, int digitLength) => 
        digits / Math.Pow(10, digitLength);
}