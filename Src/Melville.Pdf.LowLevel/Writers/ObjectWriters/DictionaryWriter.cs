using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class DictionaryWriter
{
    private static ReadOnlySpan<byte> StandardPrefix => "<<"u8();
    private static ReadOnlySpan<byte> StandardSuffix => ">>"u8();
    private static ReadOnlySpan<byte> InlineImagePrefix => "BI"u8();
    private static ReadOnlySpan<byte> InlineImageSuffix => "\nID\n"u8();
    private static ReadOnlySpan<byte> SingleSpace => " "u8();

    public static void Write(
        in PdfObjectWriter target,
        IEnumerable<KeyValuePair<PdfDirectValue, PdfIndirectValue>> items) =>
        WriteAsync(target, items, StandardPrefix, StandardSuffix);

    public static void WriteInlineImageDict(
        in PdfObjectWriter target,
        IEnumerable<KeyValuePair<PdfDirectValue, PdfIndirectValue>> items) =>
        WriteAsync(target, items, InlineImagePrefix, InlineImageSuffix);


    private static void WriteAsync(
        in PdfObjectWriter writer,
        IEnumerable<KeyValuePair<PdfDirectValue, PdfIndirectValue>> items,
        in ReadOnlySpan<byte> prefix, in ReadOnlySpan<byte> suffix)    {
        writer.Write(prefix);
        foreach (var item in items)
        {
            writer.Write(item.Key);
            AddWhitespaceIfNeeded(writer, item.Value);
            writer.Write(item.Value);
        }
        writer.Write(suffix);
    }

    private static void AddWhitespaceIfNeeded(in PdfObjectWriter writer, PdfIndirectValue item)
    {
        if (NeedsLeadingSpace(item))
        {
            writer.Write(SingleSpace);
        }
    }

    private static bool NeedsLeadingSpace(PdfIndirectValue itemValue) =>
        !itemValue.TryGetEmbeddedDirectValue(out var dv) || NeedsLeadingSpace(dv);

        private static bool NeedsLeadingSpace(PdfDirectValue itemValue) => itemValue is
            { IsName: true } or { IsNull: true } or { IsBool: true };
}