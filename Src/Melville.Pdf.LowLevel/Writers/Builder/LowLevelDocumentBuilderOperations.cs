using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public static class LowLevelDocumentBuilderOperations
    {
        public static void AddRootElement(
            this ILowLevelDocumentBuilder creator, PdfDictionary rootElt) =>
            creator.AddToTrailerDictionary(KnownNames.Root, creator.Add(rootElt));

        public static PdfDictionary NewDictionary(this ILowLevelDocumentBuilder _, params
            (PdfName Name, PdfObject Value)[] items) =>
            new(PairsToDictionary(items));

        private static Dictionary<PdfName, PdfObject> PairsToDictionary(
            IEnumerable<(PdfName Name, PdfObject Value)> items) =>
            new(
                items.Select(i => new KeyValuePair<PdfName, PdfObject>(i.Name, i.Value)));

        public static ValueTask<PdfStream> NewCompressedStream(this ILowLevelDocumentBuilder? _,
            StreamDataSource data, PdfObject encoding, PdfObject? parameters = null,
            params (PdfName Name, PdfObject Value)[] items) =>
            _.NewCompressedStream(data, encoding, parameters, items.AsEnumerable());
      
        public static async ValueTask<PdfStream> NewCompressedStream(this ILowLevelDocumentBuilder? _,
            StreamDataSource data, PdfObject encoding, PdfObject? parameters,
            IEnumerable<(PdfName Name, PdfObject Value)> items) =>
            NewStream(_, await Encode.Compress(data, encoding, parameters),
                AddEncodingValues(items, encoding, parameters));

        private static IEnumerable<(PdfName, PdfObject)> AddEncodingValues(
            IEnumerable<(PdfName Name, PdfObject Value)> items, PdfObject encoding, PdfObject? parameters) =>
            items.Append((KnownNames.Filter, encoding))
                .Append((KnownNames.DecodeParms, parameters ?? PdfTokenValues.Null));

        public static ValueTask<PdfStream> NewCompressedStream(this ILowLevelDocumentBuilder _,
            Func<Stream, ValueTask> addData, PdfObject encoding, PdfObject? parameters = null,
            params (PdfName Name, PdfObject Value)[] items) =>
            NewCompressedStream(_, addData, encoding, parameters, items.AsEnumerable());

        public static async ValueTask<PdfStream> NewCompressedStream(this ILowLevelDocumentBuilder? _,
            Func<Stream, ValueTask> addData, PdfObject encoding, PdfObject? parameters,
            IEnumerable<(PdfName Name, PdfObject Value)> items)
        {
            var ms = new MultiBufferStream();
            await using (var target = await Encode.CompressOnWrite(ms, encoding, parameters))
            {
                await addData(target);
            }

            return NewStream(_, ms,
                items.Concat(new[]
                {
                    (KnownNames.Filter, encoding),
                    (KnownNames.DecodeParms, parameters ?? PdfTokenValues.Null)
                })
            );
        }

        public static PdfStream NewStream(
            this ILowLevelDocumentBuilder? _, in StreamDataSource streamData,
            params (PdfName Name, PdfObject Value)[] items) =>
            NewStream(_, streamData, items.AsEnumerable());

        public static PdfStream NewStream(
            this ILowLevelDocumentBuilder? _, in StreamDataSource streamData,
            IEnumerable<(PdfName Name, PdfObject Value)> items)
        {
            return new(StreamDictionary(items, (int)streamData.Stream.Length),
                new LiteralStreamSource(streamData.Stream));
        }

        private static Dictionary<PdfName, PdfObject> StreamDictionary(
            IEnumerable<(PdfName Name, PdfObject Value)> items, int length) =>
            PairsToDictionary(items
                .Where(NotAnEmptyObject)
                .Append((KnownNames.Length, new PdfInteger(length))));

        private static bool NotAnEmptyObject((PdfName Name, PdfObject Value) arg) =>
            !(arg.Value == PdfTokenValues.Null ||
              arg.Value is PdfArray { Count: 0 } ||
              arg.Value is PdfDictionary { Count: 0 });

        public static PdfStream NewStream(this ILowLevelDocumentBuilder _, byte[] streamData, params
            (PdfName Name, PdfObject Value)[] items) =>
            new(StreamDictionary(items, streamData.Length), new LiteralStreamSource(streamData));
    }
}