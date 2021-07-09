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
            if (!WholeNumberParser.TryParsePositiveWholeNumber(ref source, out value, out var nextByte))
                return false;
            return nextByte == '.' ? 
                TryParseFractionalDigits(ref source) : 
                ReturnIntegerResult(ref source);
        }

        private bool ReturnIntegerResult(ref SequenceReader<byte> source)
        {
            source.Rewind(1);
            result = new PdfInteger(sign * value);
            return true;
        }

        private bool TryParseFractionalDigits(ref SequenceReader<byte> source)
        {
            placeValue = 1.0;
            while (true)
            {
                if (!source.TryRead(out var character)) return false;
                if (!WholeNumberParser.IsDigit(character)) return ReturnDoubleResult(ref source);
                ConsumeDecimalNumberPart(character);
            }
        }

        private bool ReturnDoubleResult(ref SequenceReader<byte> source)
        {
            source.Rewind(1);
            result = new PdfDouble(sign * (value + fractionalPart));
            return true;
        }

        private void ConsumeDecimalNumberPart(byte character)
        {
            placeValue /= 10.0;
            fractionalPart += placeValue * (character - (byte) '0');
        }
    }
}