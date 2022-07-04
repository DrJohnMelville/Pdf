using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

public static class DictionaryWriter
{
    private static readonly byte[] standardPrefix = { (byte)'<', (byte)'<' };
    private static readonly byte[] standardSuffix = { (byte)'>', (byte)'>' };
    private static readonly byte[] inlineImagePrefix = { (byte)'B', (byte)'I' };
    private static readonly byte[] inlineImageSuffix = {(byte)'\n', (byte)'I', (byte)'D', (byte)'\n' };

    public static ValueTask<FlushResult> WriteAsync(
        PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter,
        IReadOnlyDictionary<PdfName, PdfObject> items) =>
        WriteAsync(writer, innerWriter, items, standardPrefix, standardSuffix);
    public static ValueTask<FlushResult> WriteInlineImageDict(
        PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter,
        IReadOnlyDictionary<PdfName, PdfObject> items) =>
        WriteAsync(writer, innerWriter, items, inlineImagePrefix, inlineImageSuffix);

    private static async ValueTask<FlushResult> WriteAsync(
        PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter, 
        IReadOnlyDictionary<PdfName, PdfObject> items, byte[] prefix, byte[] suffix)
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