using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters
{
    public static class DictionaryWriter
    {
        public static async ValueTask<FlushResult> Write(
            PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter, 
            IReadOnlyDictionary<PdfName, PdfObject> items)
        {
            writer.Advance(WriteTwoBytes(writer.GetSpan(2), (byte)'<'));
            int position = 0;
            foreach (var item in items)
            {
                if (position++ > 0)
                {
                    writer.Advance(WriteByte(writer.GetSpan(1), (byte)' '));
                }

                await item.Key.Visit(innerWriter);
                writer.Advance(WriteByte(writer.GetSpan(1), (byte)' '));
                await writer.FlushAsync();
                await item.Value.Visit(innerWriter);
            }
            writer.Advance(WriteTwoBytes(writer.GetSpan(2), (byte)'>'));
            return await writer.FlushAsync();
        }

        private static int WriteByte(Span<byte> target, byte c)
        {
            target[0] = c;
            return 1;
        }
        private static int WriteTwoBytes(Span<byte> target, byte c)
        {
            target[0] = c;
            target[1] = c;
            return 2;
        }
    }
}