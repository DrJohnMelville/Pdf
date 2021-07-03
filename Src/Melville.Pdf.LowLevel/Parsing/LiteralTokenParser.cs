using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Melville.Pdf.LowLevel.Model;

namespace Melville.Pdf.LowLevel.Parsing
{
    public static class LiteralTokenParser
    {
        public static bool TryParse(ref SequenceReader<byte> input, [NotNullWhen(true)] out PdfObject? output)
        {
            if (input.TryRead(out var firstChar))
                return ByteToLiteralObject(ref input, out output, firstChar);
            output = null;
            return false;
        }

        private static bool ByteToLiteralObject(
            ref SequenceReader<byte> input, out PdfObject? output, byte firstChar) =>
            firstChar switch
            {
                (byte) 't' => VerifyToken(ref input, 3, PdfBoolean.True, out output),
                (byte) 'f' => VerifyToken(ref input, 4, PdfBoolean.False, out output),
                _ => VerifyToken(ref input, 3, PdfNull.Instance, out output)
            };
        private static bool VerifyToken(
            ref SequenceReader<byte> input, int length, PdfObject item, out PdfObject? result )
        {
            result = item;
            return input.TryAdvance(length) && NextTokenFinder.SkipToNextToken(ref input);
        }
    }
}