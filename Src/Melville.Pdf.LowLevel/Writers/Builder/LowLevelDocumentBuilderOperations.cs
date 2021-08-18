using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters;
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

        
        public static async ValueTask<PdfStream> NewCompressedStream(this ILowLevelDocumentBuilder _, 
             StreamDataSource data, PdfObject compression, PdfObject? parameters = null)
        {
            return NewStream(_, await Encode.Compress(data, compression, parameters),
                (KnownNames.Filter, compression),
                (KnownNames.DecodeParms, parameters??PdfTokenValues.Null));
        }
        public static PdfStream NewStream(this ILowLevelDocumentBuilder _, in StreamDataSource streamData, params
            (PdfName Name, PdfObject Value)[] items)
        {
            var destination = new MemoryStream();
            streamData.Stream.CopyTo(destination);
            return new(StreamDictionary(items, (int)destination.Length),
                new LiteralStreamSource(destination.ToArray()));
        }

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