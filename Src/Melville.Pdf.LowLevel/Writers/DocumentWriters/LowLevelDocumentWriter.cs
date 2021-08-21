using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public class LowLevelDocumentWriter
    {
        private readonly CountingPipeWriter target;

        public LowLevelDocumentWriter(PipeWriter target)
        {
            this.target = new CountingPipeWriter(target);
        }

        public async Task WriteAsync(PdfLowLevelDocument document)
        {
            var objectOffsets = await WriteHeaderAndObjects(document);
            long xRefStart = target.BytesWritten;
            await NewXrefTableWriter.WriteXrefsForNewFile(target, objectOffsets);
            await TrailerWriter.WriteTrailerWithDictionary(target, document.TrailerDictionary, xRefStart);
        }

        public async Task WriteWithReferenceStream(PdfLowLevelDocument document)
        {
            document.VerifyCanSupportObjectStreams();
            var objectOffsets = await WriteHeaderAndObjects(document);
            long xRefStart = target.BytesWritten;
            await new ReferenceStreamWriter(target, document, objectOffsets, xRefStart).Write();
            await TrailerWriter.WriteTerminalStartXrefAndEof(target, xRefStart);
        }

        private async Task<XRefTable> WriteHeaderAndObjects(PdfLowLevelDocument document)
        {
            HeaderWriter.WriteHeader(target, document.MajorVersion, document.MinorVersion);
            var objectOffsets = await WriteObjectList(document);
            return objectOffsets;
        }

        private async Task<XRefTable> WriteObjectList(PdfLowLevelDocument document)
        {
            var positions= CreateIndexArray(document);
            var objectWriter = new PdfObjectWriter(target);
            foreach (var item in document.Objects.Values)
            {
                if (!(await item.Target.DirectValue()).ShouldWriteToFile()) continue;
                positions.DeclareIndirectObject(item.Target.ObjectNumber, target.BytesWritten);
                await item.Target.Visit(objectWriter);
            }
            return positions;
        }

        private  XRefTable CreateIndexArray(PdfLowLevelDocument document)
        {
            var maxObject = document.Objects.Keys.Max(i => i.ObjectNumber);
            return new XRefTable(maxObject + 1);
        }
    }
}