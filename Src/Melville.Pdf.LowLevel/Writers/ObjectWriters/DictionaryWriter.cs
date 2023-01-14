using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class DictionaryWriter
{
    private static readonly byte[] StandardPrefix = "<<"u8.ToArray();
    private static readonly byte[] StandardSuffix = ">>"u8.ToArray();
    private static readonly byte[] InlineImagePrefix = "BI"u8.ToArray();
    private static readonly byte[] InlineImageSuffix = "\nID\n"u8.ToArray();

    public static ValueTask<FlushResult> WriteAsync(
        PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter,
        IEnumerable<KeyValuePair<PdfName, PdfObject>> items) =>
        WriteAsync(writer, innerWriter, items, StandardPrefix, StandardSuffix);
    public static ValueTask<FlushResult> WriteInlineImageDict(
        PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter,
        IEnumerable<KeyValuePair<PdfName, PdfObject>> items) =>
        WriteAsync(writer, innerWriter, items, InlineImagePrefix, InlineImageSuffix);

    private static async ValueTask<FlushResult> WriteAsync(
        PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter, 
        IEnumerable<KeyValuePair<PdfName, PdfObject>> items, byte[] prefix, byte[] suffix)
    {
        writer.WriteBytes(prefix);
        foreach (var item in items)
        {
            await item.Key.Visit(innerWriter).CA();
            AddWhitespaceIfNeeded(writer, item.Value); 
            await item.Value.Visit(innerWriter).CA();
            await writer.FlushAsync().CA();
        }
        writer.WriteBytes(suffix);
        return await writer.FlushAsync().CA();
    }

    private static void AddWhitespaceIfNeeded(PipeWriter writer, PdfObject item)
    {
        if (NeedsLeadingSpace(item))
        {
            writer.WriteSpace();
        }
    }

    private static bool NeedsLeadingSpace(PdfObject itemValue) => 
        itemValue is PdfNumber or PdfIndirectObject
            or PdfTokenValues;
}