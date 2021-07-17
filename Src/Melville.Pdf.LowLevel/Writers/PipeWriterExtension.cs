using System;
using System.IO.Pipelines;

namespace Melville.Pdf.LowLevel.Writers
{
    public static class PipeWriterExtension
    {
        public static void WriteBytes(this PipeWriter pw, byte[] text)
        {
            var span = pw.GetSpan(text.Length);
            text.AsSpan().CopyTo(span);
            pw.Advance(text.Length);
        }
    }
}