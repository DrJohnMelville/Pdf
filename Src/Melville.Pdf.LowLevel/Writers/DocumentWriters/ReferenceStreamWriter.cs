using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

internal readonly struct ReferenceStreamWriter
{
    private readonly PipeWriter target;
    private readonly PdfLowLevelDocument document;
    private readonly XRefTable objectOffsets;
    private readonly (int, int, int) columnWidths;

    public ReferenceStreamWriter(
        PipeWriter target, PdfLowLevelDocument document, XRefTable objectOffsets)
    {
        this.target = target;
        this.document = document;
        this.objectOffsets = objectOffsets;
        objectOffsets.AssembleFreeList();
        columnWidths = objectOffsets.ColumnByteWidths();
    }

    public async  ValueTask<FlushResult> WriteAsync()
    {
        await UnencryptedPdfObjectWriter().WriteTopLevelDeclarationAsync(XrefStreamObjectNumber(), 0,
                await CreateReferenceStreamAsync().CA()).CA();
        return await target.FlushAsync().CA();
    }

    private PdfObjectWriter UnencryptedPdfObjectWriter() => new(target);

    private int XrefStreamObjectNumber() => objectOffsets.Entries.Length-1;

    private async ValueTask<PdfValueStream> CreateReferenceStreamAsync()
    {
        var data = new MultiBufferStream(2048);
        await GenerateXrefStreamAsync(data).CA();

        return new ValueDictionaryBuilder()
            .WithMultiItem(document.TrailerDictionary.RawItems
                .Where(i => !i.Key.Equals(KnownNames.SizeTName))
                .Select(i=> new KeyValuePair<PdfDirectValue, PdfIndirectValue>(i.Key, i.Value)))
            .WithItem(KnownNames.TypeTName, KnownNames.XRefTName)
            .WithItem(KnownNames.WTName, WidthsAsArray())
            .WithItem(KnownNames.SizeTName, objectOffsets.Entries.Length)
            .WithFilter(FilterName.FlateDecode)
            .WithFilterParam(FilterParam())
            .AsStream(data);
    }

    private PdfValueDictionary FilterParam() => new ValueDictionaryBuilder()
        .WithItem(KnownNames.PredictorTName, 12)
        .WithItem(KnownNames.ColumnsTName, columnWidths.Item1 + columnWidths.Item2 + columnWidths.Item3)
        .AsDictionary();

    private PdfDirectValue WidthsAsArray() =>
        new PdfValueArray(
            columnWidths.Item1,
            columnWidths.Item2,
            columnWidths.Item3
        );
    private async ValueTask GenerateXrefStreamAsync(Stream arg)
    {
        var w = GenerateXrefStream(arg);
        await w.FlushAsync().CA();
    }

    private PipeWriter GenerateXrefStream(Stream arg)
    {
        var writer = PipeWriter.Create(arg);
        var pos1 = columnWidths.Item1;
        var pos2 = pos1 + columnWidths.Item2;
        var rowWidth = pos2 + columnWidths.Item3;
        foreach (var entry in objectOffsets.Entries)
        {
            var span = writer.GetSpan(rowWidth);
            PutInteger(span, entry.Type, columnWidths.Item1);
            PutInteger(span.Slice(pos1), entry.Column1, columnWidths.Item2);
            PutInteger(span.Slice(pos2), entry.Column2, columnWidths.Item3);
            writer.Advance(rowWidth);
        }
        return writer;
    }

    private void PutInteger(Span<byte> span, long value, int bytes)
    {
        for (int i = bytes-1; i >= 0; i--)
        {
            span[i] = (byte)value;
            value >>= 8;
        }
        Debug.Assert(value == 0, "Field was not wide enough to express value");
    }
}