using System.Buffers;

namespace Melville.Pdf.LowLevel.Model.Primitives;

public ref struct NumberWtihFractionParser
{
    private long value;
    private int sign;
    private double fractionalPart;
    private double placeValue;

    public bool HasFractionalPart() => fractionalPart != 0;
    public long IntegerValue() => sign * value;
    public double DoubleValue() => sign * (value + fractionalPart);

    public bool InnerTryParse(ref SequenceReader<byte> source, bool final)
    {
        fractionalPart = 0.0;
        return TryReadSign(ref source) && TryParseWholeDigits(ref source, final);
    }

    private bool TryReadSign(ref SequenceReader<byte> source)
    {
        sign = 1;
        if (!source.TryPeek(out var character)) return false;
        switch (character)
        {
            case (int)'+':
                source.Advance(1);
                break;
            case (int)'-':
                source.Advance(1);
                sign = -1;
                break;
        }
        return true;
    }

    private bool TryParseWholeDigits(ref SequenceReader<byte> source, bool final)
    {
        if (!WholeNumberParser.TryParsePositiveWholeNumber(ref source, out value, out var nextByte))
            return final;
        return nextByte == '.' ? 
            TryParseFractionalDigits(ref source, final) : 
            PushbackTerminatingCharacter(ref source);
    }

    private bool PushbackTerminatingCharacter(ref SequenceReader<byte> source)
    {
        source.Rewind(1);
        return true;
    }

    private bool TryParseFractionalDigits(ref SequenceReader<byte> source, bool final)
    {
        placeValue = 1.0;
        while (true)
        {
            if (!source.TryRead(out var character)) return final;
            if (!WholeNumberParser.IsDigit(character)) 
                return PushbackTerminatingCharacter(ref source);
            ConsumeDecimalNumberPart(character);
        }
    }

    private void ConsumeDecimalNumberPart(byte character)
    {
        placeValue /= 10.0;
        fractionalPart += placeValue * (character - (byte) '0');
    }
}