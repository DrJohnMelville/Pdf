using System;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
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
            var objectOffsets = await WriteHeaderAndObjects(document, 1);
            long xRefStart = target.BytesWritten;
            objectOffsets.DeclareIndirectObject(objectOffsets.Entries.Length-1, xRefStart);
            await new ReferenceStreamWriter(target, document, objectOffsets).Write();
            await TrailerWriter.WriteTerminalStartXrefAndEof(target, xRefStart);
        }

        private async Task<XRefTable> WriteHeaderAndObjects(
            PdfLowLevelDocument document, int extraSlots = 0)
        {
            HeaderWriter.WriteHeader(target, document.MajorVersion, document.MinorVersion);
            var objectOffsets = await WriteObjectList(document, extraSlots);
            return objectOffsets;
        }

        private async Task<XRefTable> WriteObjectList(PdfLowLevelDocument document, int extraSlots)
        {
            var positions= CreateIndexArray(document, extraSlots);
            var objectWriter = new PdfObjectWriter(target);
            foreach (var item in document.Objects.Values)
            {
                if (!(await item.Target.DirectValue()).ShouldWriteToFile()) continue;
                positions.DeclareIndirectObject(item.Target.ObjectNumber, target.BytesWritten);
                if (await item.DirectValue() is IHasInternalIndirectObjects hiid)
                {
                    int streamPosition = 0;
                    foreach (var innerObjectNumber in await hiid.GetInternalObjectNumbersAsync())
                    {
                        EnsureOuterGenerationNumberIsZero(item.Target);
                        positions.DeclareObjectStreamObject(
                            innerObjectNumber.ObjectNumber, item.Target.ObjectNumber, streamPosition++);
                    }
                }
                await item.Target.Visit(objectWriter);
            }
            return positions;
        }

        private void EnsureOuterGenerationNumberIsZero(PdfIndirectObject itemTarget)
        {
            if (itemTarget.GenerationNumber != 0)
                throw new InvalidOperationException("Object streams must hae a generation number of 0.");
        }

        private XRefTable CreateIndexArray(PdfLowLevelDocument document, int extraSlots)
        {
#warning -- this is worng -- the maximum object could be in an object stream
            var maxObject = document.Objects.Keys.Max(i => i.ObjectNumber);
            return new XRefTable(maxObject + 1 + extraSlots);
        }
    }
}