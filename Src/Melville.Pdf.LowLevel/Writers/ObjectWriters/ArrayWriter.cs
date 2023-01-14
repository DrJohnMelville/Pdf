using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class ArrayWriter
{
    public static async ValueTask<FlushResult> Write(
        PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter, 
        IReadOnlyList<PdfObject> items)
    {
        writer.WriteByte((byte)'[');
        var count = items.Count;
        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                writer.WriteSpace();
            }
            await items[i].Visit(innerWriter).CA();
        }
        writer.WriteByte( (byte)']');
        return await writer.FlushAsync().CA();
    }

    private static int WriteByte(Span<byte> target, byte c)
    {
        target[0] = c;
        return 1;
    }
}