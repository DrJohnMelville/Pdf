using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model;

namespace Melville.Pdf.LowLevel.Parsing
{
    public static class HexStringParser
        {
            public static bool TryParse(
                ref SequenceReader<byte> input, [NotNullWhen(true)] out PdfString? output)
            {
            output = null;
            if (input.Remaining == 0) return false;
            if (!input.TryReadTo(out ReadOnlySpan<byte> digits, (byte) '>', true)) return false;
            var buff = ComputeStringAsBuffer(ref digits);
            output = new PdfString(buff);
            return NextTokenFinder.SkipToNextToken(ref input);
        }

        private static byte[] ComputeStringAsBuffer(ref ReadOnlySpan<byte> digits)
        {
            var buff = new byte[CountChars(ref digits)];
            
            byte priorDigit = 255;
            int finalPos = 0;
            foreach (var rawByte in digits)
            {
                switch (priorDigit, HexValue(rawByte))
                {
                    case (_, 255): break;
                    case (255, var first):
                        priorDigit = first;
                        break;
                    case var (_, second):
                        AddByteToString(buff, finalPos++, priorDigit, second);
                        priorDigit = 255;
                        break;
                }
            }

            AdjustForTrailingSingleDigit(priorDigit, buff);
            return buff;
        }

        private static void AddByteToString(byte[] buff, int index, byte priorDigit, byte second) => 
            buff[index] = ByteFromNibbles(priorDigit, second);

        private static void AdjustForTrailingSingleDigit(byte priorDigit, byte[] buff)
        {
            if (priorDigit < 255)
                AddByteToString(buff, buff.Length-1, priorDigit, 0);
        }

        private static byte ByteFromNibbles(byte mostSig, byte leastSig) => 
            (byte)((mostSig << 4) | leastSig);

        private static int CountChars(ref ReadOnlySpan<byte> digits)
        {
            int bytes = 0;
            for (int i = 0; i < digits.Length; i++)
            {
                if (HexValue(digits[i]) < 255) bytes++;
            }
            return IncrementIfOdd(bytes) / 2;
        }

        private static int IncrementIfOdd(int bytes) => bytes + (bytes & 0x1);

        private static byte HexValue(byte digit) =>
            digit switch
            {
                >= (byte) '0' and <= (byte) '9' => (byte) (digit - (byte) '0'),
                >= (byte) 'A' and <= (byte) 'F' => (byte) (digit - ((byte) 'A' - 10)),
                _ => 255
            };
    }
}