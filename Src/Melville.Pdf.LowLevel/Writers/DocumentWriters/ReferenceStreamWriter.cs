using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public readonly struct ReferenceStreamWriter
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

        public async  ValueTask<FlushResult> Write() =>
            await new PdfIndirectObject(XrefStreamObjectNumber(), 0, await CreateReferenceStream())
                .Visit(new PdfObjectWriter(target));

        private int XrefStreamObjectNumber() => objectOffsets.Entries.Length-1;

        private async ValueTask<PdfStream> CreateReferenceStream()
        {
            return await LowLevelDocumentBuilderOperations.NewCompressedStream(
                null, GenerateXrefStreamAsync, KnownNames.FlateDecode, PdfTokenValues.Null, DictionaryItems());
        }
        
        private IEnumerable<(PdfName, PdfObject)> DictionaryItems() =>
            document.TrailerDictionary.RawItems
                .Where(i=>i.Key != KnownNames.Size)
                .Select(i=>(i.Key, i.Value))
                .Concat(new (PdfName, PdfObject)[]
                {
                    (KnownNames.Type, KnownNames.XRef),
                    (KnownNames.W, WidthsAsArray()),
                    (KnownNames.Size, new PdfInteger(objectOffsets.Entries.Length))
                });

        private PdfObject WidthsAsArray() =>
            new PdfArray(
                new PdfInteger(columnWidths.Item1),
                new PdfInteger(columnWidths.Item2),
                new PdfInteger(columnWidths.Item3)
            );
        private ValueTask GenerateXrefStreamAsync(Stream arg) => 
            GenerateXrefStream(arg).FlushAsync().AsValueTask();

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

    public static class ValueTaskStripper
    {
        [Obsolete("This belongs in melville.Hacks")]
        public static ValueTask AsValueTask<T>(this ValueTask<T> valueTask)
        {
            if (valueTask.IsCompletedSuccessfully)
            {
                valueTask.GetAwaiter().GetResult();
                return default;
            }

            return new ValueTask(valueTask.AsTask());
        }
    }
}