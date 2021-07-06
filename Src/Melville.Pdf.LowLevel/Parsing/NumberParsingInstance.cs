using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.NameParsing;

namespace Melville.Pdf.LowLevel.Parsing
{
    public class NumberParser: PdfAtomParser
    {
        public override bool TryParse(ref SequenceReader<byte> reader, out PdfObject? obj) => 
            new NumberParsingInstance().InnerTryParse(ref reader, out obj);
    }

    public ref struct NumberParsingInstance
    {
        private int value;
        private int sign;
        private double fractionalPart;
        private double placeValue;
        private PdfNumber? result;

        public bool InnerTryParse(
            ref SequenceReader<byte> source, [NotNullWhen(true)] out PdfObject? output)
        {
            var ret = TryReadSign(ref source) && TryParseWholeDigits(ref source);
            output = result;
            return ret;
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

        private bool TryParseWholeDigits(
            ref SequenceReader<byte> source)
        {
            while (true)
            {
                if (!source.TryRead(out var character)) return false;
                switch (character)
                {
                    case >= (byte) '0' and <= (byte) '9':
                        ConsumeWholeNumberPart(character);
                        break;
                    case (int)'.':
                        return TryParseFractionalDigits(ref source);
                    default:
                        source.Rewind(1);
                        return TryCompleteNumberParse(ref source);
                }
            }
        }
        
        private void ConsumeWholeNumberPart(byte character)
        {
            value *= 10;
            value += character - (byte) '0';
        }
        
        private bool TryParseFractionalDigits(ref SequenceReader<byte> source)
        {
            placeValue = 1.0;
            while (true)
            {
                if (!source.TryRead(out var character)) return false;
                switch (character)
                {
                    case >= (byte) '0' and <= (byte) '9':
                        ConsumeDecimalNumberPart(character);
                        break;
                    default:
                        source.Rewind(1);
                        return TryCompleteNumberParse(ref source);
                }
            }
        }

        private void ConsumeDecimalNumberPart(byte character)
        {
            placeValue /= 10.0;
            fractionalPart += placeValue * (character - (byte) '0');
        }

        private bool TryCompleteNumberParse(ref SequenceReader<byte> source)
        {
            CreateParsedNumber();
            return NextTokenFinder.SkipToNextToken(ref source);
        }

        private void CreateParsedNumber() => 
            result= fractionalPart == 0.0 ? 
                new PdfInteger(sign * value) : 
                new PdfDouble(sign * (value + fractionalPart));
    }
}