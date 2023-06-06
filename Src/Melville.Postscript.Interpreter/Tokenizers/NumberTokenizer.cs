using System;
using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal ref partial struct NumberTokenizer
{
    [FromConstructor] private readonly int radix;
    private int sign = 1;
    private long number;
    private long denominator = 1;

    public bool TryParse(ref SequenceReader<byte> source, out PostscriptValue result)
    {
        if (!TryParseSignedNumber(ref source, out var terminator))
            return ReturnViaOut.FalseDefault(out result);
        return terminator switch
        {
            (int)'#' => ParseRadixNumber(ref source, out result),
            (int)'.' => ParseDouble(ref source, out result),
            (int)'e' or (int)'E' => ParseDoubleWithoutPeriod(ref source, out result),
            _ => PostscriptValueFactory.Create(AsLongValue()).AsTrueValue(out result)
        };
    }

    private bool TryParseSignedNumber(ref SequenceReader<byte> source, out byte terminator)
    {
        terminator = 0;
        return TryParseSign(ref source, out sign) &&
               TryParseWholeNumber(ref source, out terminator);
    }


    private bool TryParseWholeNumber(ref SequenceReader<byte> source, out byte terminator)
    {
        while (true)
        {
            if (!source.TryPeek(out terminator)) return false;
            var digitValue = ValueFromDigit(terminator);
            if (digitValue >= radix) return true;
            AddDigit(digitValue);
            source.Advance(1);
        }
    }

    private long ValueFromDigit(byte digitChar) => digitChar switch
    {
        >= (int)'0' and <= (int)'9' => digitChar - '0',
        >= (int)'A' and <= (int)'Z' => digitChar - 'A' + 10,
        >= (int)'a' and <= (int)'z' => digitChar - 'a' + 10,
        _ => int.MaxValue
    };


    private void AddDigit(long digit)
    {
        number *= radix;
        number += digit;
        denominator *= radix;
    }

    private bool ParseRadixNumber(ref SequenceReader<byte> source, out PostscriptValue result)
    {
        source.Advance(1);
        var subParser = new NumberTokenizer((int)number);
        return subParser.TryParseWholeNumber(ref source, out var _) ? 
            PostscriptValueFactory.Create(subParser.AsLongValue()).AsTrueValue(out result) : 
            ReturnViaOut.FalseDefault(out result);
    }

    private bool ParseDouble(ref SequenceReader<byte> source, out PostscriptValue result)
    {
        source.Advance(1);
        return ParseDoubleWithoutPeriod(ref source, out result);
    }

    private bool ParseDoubleWithoutPeriod(ref SequenceReader<byte> source, out PostscriptValue result)
    {
        var fractionTokenizer = new NumberTokenizer(10);
        return (fractionTokenizer.TryParseWholeNumber(ref source, out var terminator) &&
                ReadExponent(ref source, terminator, out var exponent))
            ? fractionTokenizer.ProduceDouble(number, sign * exponent, out result)
            : ReturnViaOut.FalseDefault(out result);
    }

    private bool ReadExponent(ref SequenceReader<byte> source, byte terminator, out double value)
    {
        if (terminator is not ((int)'E' or (int)'e')) return 1.0.AsTrueValue(out value);
        source.Advance(1);
        var exponentTokenizer = new NumberTokenizer(10);
        var result = exponentTokenizer.TryParseSignedNumber(ref source, out var _);
        return Math.Pow(10,exponentTokenizer.AsLongValue()).WrapTry(result, out value);
    }

    private bool TryParseSign(ref SequenceReader<byte> source, out int sign)
    {
        if (!source.TryPeek(out var signChar)) return 0.AsFalseValue(out sign);
        switch (signChar)
        {
            case (int)'+':
                source.Advance(1);
                break;
            case (int)'-':
                source.Advance(1);
                return (-1).AsTrueValue(out sign);
        }
        return 1.AsTrueValue(out sign);
    }

    private bool ProduceDouble(long wholeNumber, double factor, out PostscriptValue result) => 
        PostscriptValueFactory.Create((wholeNumber + AsFractionalValue()) * factor)
            .AsTrueValue(out result);

    private long AsLongValue() => sign * number;
    private double AsFractionalValue() => ((double)number) / denominator;
}