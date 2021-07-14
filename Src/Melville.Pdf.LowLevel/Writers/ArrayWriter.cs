using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers
{
    public static class ArrayWriter
    {
        public static async ValueTask<FlushResult> Write(
            PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter, 
            IReadOnlyList<PdfObject> items)
        {
            writer.Advance(WriteByte(writer.GetSpan(1), (byte)'['));
            var count = items.Count;
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    writer.Advance(WriteByte(writer.GetSpan(1), (byte)' '));
                }
                await items[i].Visit(innerWriter);
            }
            writer.Advance(WriteByte(writer.GetSpan(1), (byte)']'));
            return await writer.FlushAsync();
        }

        private static int WriteByte(Span<byte> target, byte c)
        {
            target[0] = c;
            return 1;
        }
    }
}