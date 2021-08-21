using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly long startXref;

        public ReferenceStreamWriter(
            PipeWriter target, PdfLowLevelDocument document, XRefTable objectOffsets, long startXref)
        {
            this.target = target;
            this.document = document;
            this.objectOffsets = objectOffsets;
            this.startXref = startXref;
        }

        public  ValueTask<FlushResult> Write()
        {
            return new PdfIndirectObject(objectOffsets.Entries.Length, 0, CreateReferenceStream())
                .Visit(new PdfObjectWriter(target));
        }

        private PdfStream CreateReferenceStream()
        {
            return LowLevelDocumentBuilderOperations.NewStream(
                null, "StreamData", DictionaryItems());
        }

        private IEnumerable<(PdfName, PdfObject)> DictionaryItems() =>
            document.TrailerDictionary.RawItems
                .Where(i=>i.Key != KnownNames.Size)
                .Select(i=>(i.Key, i.Value))
                .Concat(new (PdfName, PdfObject)[]
                {
                    (KnownNames.Type, KnownNames.XRef),
                    (KnownNames.Size, new PdfInteger(objectOffsets.Entries.Length + 1))
                });
    }
}