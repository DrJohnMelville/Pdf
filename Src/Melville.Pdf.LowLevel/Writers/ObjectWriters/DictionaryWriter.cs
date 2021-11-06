using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

public static class DictionaryWriter
{
    public static async ValueTask<FlushResult> WriteAsync(
        PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter, 
        IReadOnlyDictionary<PdfName, PdfObject> items)
    {
        WritePrefix(writer);
        foreach (var item in items)
        {
            await item.Key.Visit(innerWriter);
            AddWhitespaceIfNeeded(writer, item.Value); 
            await item.Value.Visit(innerWriter);
            await writer.FlushAsync();
        }
        WritePostfix(writer);
        return await writer.FlushAsync();
    }

    private static void WritePrefix(PipeWriter writer) => writer.WriteBytes((byte) '<', (byte) '<');

    private static void WritePostfix(PipeWriter writer) => writer.WriteBytes((byte) '>', (byte) '>');

    private static void AddWhitespaceIfNeeded(PipeWriter writer, PdfObject item)
    {
        if (NeedsLeadingSpace(item))
        {
            writer.WriteSpace();
        }
    }

    private static bool NeedsLeadingSpace(PdfObject itemValue) => 
        itemValue is PdfNumber or PdfIndirectObject or PdfIndirectReference;
}