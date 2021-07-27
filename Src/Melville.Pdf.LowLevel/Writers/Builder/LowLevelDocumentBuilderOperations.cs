using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

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

        public static PdfStream NewCompressedStream(this ILowLevelDocumentBuilder _,
            string data, PdfObject compression, PdfObject? parameters = null) =>
            _.NewCompressedStream(
                ExtendedAsciiEncoding.AsExtendedAsciiBytes(data), compression, parameters);
        
        public static PdfStream NewCompressedStream(this ILowLevelDocumentBuilder _, 
            byte[] data, PdfObject compression, PdfObject? parameters = null)
        {
            return NewStream(_, Compressor.Compress(data, compression, parameters),
                (KnownNames.Filter, compression),
                (KnownNames.Params, parameters??PdfTokenValues.Null));
        }
        public static PdfStream NewStream(this ILowLevelDocumentBuilder _, string streamData, params
            (PdfName Name, PdfObject Value)[] items) =>
            new(StreamDictionary(items, streamData.Length), new LiteralStreamSource(streamData));

        private static Dictionary<PdfName, PdfObject> StreamDictionary(
            (PdfName Name, PdfObject Value)[] items, int length)
        {
            return PairsToDictionary(items
                .Where(NotAnEmptyObject)
                .Append((KnownNames.Length, new PdfInteger(length))));
        }

        private static bool NotAnEmptyObject((PdfName Name, PdfObject Value) arg) =>
            !(arg.Value == PdfTokenValues.Null ||
              arg.Value is PdfArray {Count: 0} ||
              arg.Value is PdfDictionary {Count: 0});

        public static PdfStream NewStream(this ILowLevelDocumentBuilder _, byte[] streamData, params
            (PdfName Name, PdfObject Value)[] items) =>
            new(StreamDictionary(items, streamData.Length), new LiteralStreamSource(streamData));
    }
}