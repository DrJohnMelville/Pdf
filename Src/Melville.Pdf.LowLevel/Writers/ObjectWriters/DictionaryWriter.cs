using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class DictionaryWriter
{
    private static ReadOnlySpan<byte> StandardPrefix => "<<"u8;
    private static ReadOnlySpan<byte> StandardSuffix => ">>"u8;
  private static ReadOnlySpan<byte> InlineImagePrefix => "BI"u8;
    private static ReadOnlySpan<byte> InlineImageSuffix => "\nID\n"u8;
    private static ReadOnlySpan<byte> SingleSpace => " "u8;

    public static void Write(
        in PdfObjectWriter target,
        IEnumerable<KeyValuePair<PdfDirectObject, PdfIndirectObject>> items) =>
        Write(target, items, StandardPrefix, StandardSuffix);

    public static void WriteInlineImageDict(
        in PdfObjectWriter target,
        IEnumerable<KeyValuePair<PdfDirectObject, PdfIndirectObject>> items) =>
        Write(target, items, InlineImagePrefix, InlineImageSuffix);


    private static void Write(
        in PdfObjectWriter writer,
        IEnumerable<KeyValuePair<PdfDirectObject, PdfIndirectObject>> items,
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

    private static void AddWhitespaceIfNeeded(in PdfObjectWriter writer, PdfIndirectObject item)
    {
        if (item.NeedsLeadingSpace())
        {
            writer.Write(SingleSpace);
        }
    }

}