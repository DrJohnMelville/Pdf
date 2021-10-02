using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters
{
    public static class StringWriter
    {
 
        public static ValueTask<FlushResult> Write(
            PipeWriter writer, PdfString value, IObjectCryptContext encryptor)
        {
            var buffer = writer.GetSpan( MaaximumRenderedStringLength(value));
            var encrypted = encryptor.StringCipher().Encrypt().CryptSpan(value.Bytes);
            var len = CopyToSpan(ref buffer, encrypted);
            writer.Advance(len);
            return writer.FlushAsync();
        }

        private const int SpaceForOpenAndClosedParens = 2;
        private static int MaaximumRenderedStringLength(PdfString value)
        {
            //every character could be special and therefore require 2 bytes to render.
            return SpaceForOpenAndClosedParens+(2*value.Bytes.Length);
        }

        private static int CopyToSpan(ref Span<byte> buffer, ReadOnlySpan<byte> valueBytes)
        {
            int pos = 0;
            buffer[pos++] = (byte)'(';
            foreach (var item in valueBytes)
            {
                var (isSpecial, finalByte) = IsSpecialByte(item);
                if (isSpecial)
                {
                    buffer[pos++] = (byte) '\\';
                }

                buffer[pos++] = finalByte;
            }
            buffer[pos++] = (byte)')';
            return pos;
        }

        private static (bool isSpecial, byte suffix) IsSpecialByte(byte input) => input switch
        {
            (byte)'(' or (byte)')' or (byte)'\\' => (true, input),
            (byte)'\n' => (true, (byte)'n'),
            (byte)'\r' => (true, (byte)'r'),
            (byte)'\t' => (true, (byte)'t'),
            (byte)'\b' => (true, (byte)'b'),
            (byte)'\f' => (true, (byte)'f'),
            _ => (false, input)
        };
    }
}