using System;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal readonly partial struct  RadixPrinter
{
    [FromConstructor] private readonly PostscriptValue number;
    [FromConstructor] private readonly int radix;
    [FromConstructor] private readonly Memory<byte> target;

    public PostscriptValue CreateValue()
    {
        return radix == 10 ? PrintBase10() : PrintBaseRadix();
    }

    private PostscriptValue PrintBase10()
    {
        var output = number.ToString();
        if (output.Length > target.Length)
            throw new PostscriptException("Output too long for buffer");
        var length = Encoding.ASCII.GetBytes(output, target.Span);
        return SliceResult(length);
    }

    private PostscriptValue SliceResult(int length)
    {
        return PostscriptValueFactory.CreateLongString(
            target.Slice(0, length), StringKind.String);
    }

    private PostscriptValue PrintBaseRadix()
    {
        var value = number.Get<long>();
        if (value == 0)
        {
            target.Span[0] = (byte)'0';
            return SliceResult(1);
        }
        int length = PrintDigit(AdjustForNegative(value));
        return SliceResult(length);
    }

    private static long AdjustForNegative(long value) => 
        value < 0 ? (long)(uint)(int)value:value;

    private int PrintDigit(long number)
    {
        if (number == 0) return 0;
        var (quotient, remainder) = long.DivRem(number, radix);
        int position = PrintDigit(quotient);
        target.Span[position] = DigitFor(remainder);
        return position + 1;
    }

    private byte DigitFor(long remainder) => (byte)(remainder switch
    {
        < 10 => remainder + '0',
        _ => remainder + 'A' - 10,
    });
}