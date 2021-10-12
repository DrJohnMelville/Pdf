using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public static class LowLevelDocumentBuilderOperations
    {
        public static void AddRootElement(
            this ILowLevelDocumentBuilder creator, PdfDictionary rootElt) =>
            creator.AddToTrailerDictionary(KnownNames.Root, creator.Add(rootElt));

        public static PdfStream NewCompressedStream(this ILowLevelDocumentBuilder? _,
            StreamDataSource data, PdfObject encoding, PdfObject? parameters = null,
            params (PdfName Name, PdfObject Value)[] items) =>
            _.NewCompressedStream(data, encoding, parameters, items.AsEnumerable());
      
        public static PdfStream NewCompressedStream(this ILowLevelDocumentBuilder? _,
            StreamDataSource data, PdfObject encoding, PdfObject? parameters,
            IEnumerable<(PdfName Name, PdfObject Value)> items) =>
            NewStream(_, data,
                AddEncodingValues(items, encoding, parameters));

        private static IEnumerable<(PdfName, PdfObject)> AddEncodingValues(
            IEnumerable<(PdfName Name, PdfObject Value)> items, PdfObject encoding, PdfObject? parameters) =>
            items.Append((KnownNames.Filter, encoding))
                .Append((KnownNames.DecodeParms, parameters ?? PdfTokenValues.Null));

        public static PdfStream NewStream(
            this ILowLevelDocumentBuilder? _, in StreamDataSource streamData,
            params (PdfName Name, PdfObject Value)[] items) =>
            NewStream(_, streamData, items.AsEnumerable());

        public static PdfStream NewStream(
            this ILowLevelDocumentBuilder? _, in StreamDataSource streamData,
            IEnumerable<(PdfName Name, PdfObject Value)> items) =>
            new(new LiteralStreamSource(streamData.Stream, StreamFormat.PlainText),
                items.StripTrivialItems());
    }
}