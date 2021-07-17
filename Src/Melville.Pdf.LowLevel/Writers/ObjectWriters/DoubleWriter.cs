using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters
{
    public static class DoubleWriter
    {
        public static ValueTask<FlushResult> Write(PipeWriter target, double item)
        {
            Span<char> charSpan = stackalloc char[20];
            var span = target.GetSpan(20);
            item.TryFormat(charSpan, out var written);
            CopyCharSpanToByteSpan(written, span, charSpan);
            target.Advance(written);
            return target.FlushAsync();
        }

        private static void CopyCharSpanToByteSpan(int written, Span<byte> span, Span<char> charSpan)
        {
            for (int i = 0; i < written; i++)
            {
                span[i] = (byte) (charSpan[i]);
            }
        }
    }
}