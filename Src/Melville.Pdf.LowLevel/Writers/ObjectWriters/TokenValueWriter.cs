using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters
{
    public static class TokenValueWriter
    {
        public static ValueTask<FlushResult> Write(PipeWriter target, PdfTokenValues item)
        {
            var tokenLength = item.TokenValue.Length;
            var mem = target.GetSpan(tokenLength);
            item.TokenValue.AsSpan().CopyTo(mem);
            target.Advance(tokenLength);
            return target.FlushAsync();
        }
    }
}