using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters
{
    public static class StringWriter
    {
        public static ValueTask<FlushResult> Write(PipeWriter writer, PdfString value)
        {
            var len = CountChars(value.Bytes);
            var buffer = writer.GetSpan(len);
            CopyToSpan(ref buffer, value.Bytes);
            writer.Advance(len);
            return writer.FlushAsync();
        }

        private static void CopyToSpan(ref Span<byte> buffer, byte[] valueBytes)
        {
            int pos = 0;
            buffer[pos++] = (byte)'(';
            foreach (var item in valueBytes)
            {
                if (IsSpecialByte(item))
                {
                    buffer[pos++] = (byte) '\\';
                }

                buffer[pos++] = item;
            }
            buffer[pos] = (byte)')';
        }

        private static int CountChars(byte[] bytes)
        {
            int ret = 2; // opening and closing parens
            foreach (var item in bytes)
            {
                if (IsSpecialByte(item)) ret++;
                ret++;
            }

            return ret;
        }

        private static bool IsSpecialByte(byte item)
        {
            return item is (byte)'(' or (byte)')' or (byte)'\\';
        }
    }
}