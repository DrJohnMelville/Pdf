using System;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Encryption.Writers;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public class LowLevelDocumentWriter
    {
        private readonly CountingPipeWriter target;
        private readonly PdfLowLevelDocument document;
        private readonly string? userPassword;


        public LowLevelDocumentWriter(
            PipeWriter target, PdfLowLevelDocument document, string? userPassword = null)
        {
            this.target = new CountingPipeWriter(target);
            this.document = document;
            this.userPassword = userPassword;
        }

        public async Task WriteAsync()
        {
            
            var objectOffsets = await WriteHeaderAndObjects(0);
            long xRefStart = target.BytesWritten;
            await NewXrefTableWriter.WriteXrefsForNewFile(target, objectOffsets);
            await TrailerWriter.WriteTrailerWithDictionary(target, document.TrailerDictionary, xRefStart);
        }

        public async Task WriteWithReferenceStream()
        {
            document.VerifyCanSupportObjectStreams();
            var objectOffsets = await WriteHeaderAndObjects(1);
            long xRefStart = target.BytesWritten;
            objectOffsets.DeclareIndirectObject(objectOffsets.Entries.Length-1, xRefStart);
            await new ReferenceStreamWriter(target, document, objectOffsets).Write();
            await TrailerWriter.WriteTerminalStartXrefAndEof(target, xRefStart);
        }

        private async Task<XRefTable> WriteHeaderAndObjects( int extraSlots)
        {
            HeaderWriter.WriteHeader(target, document.MajorVersion, document.MinorVersion);
            var objectOffsets = await WriteObjectList(document, extraSlots);
            return objectOffsets;
        }

        private async Task<XRefTable> WriteObjectList(PdfLowLevelDocument document, int extraSlots)
        {
            var positions= CreateIndexArray(document, extraSlots);
            var objectWriter = new PdfObjectWriter(target,
                await TrailerToDocumentCryptContext.CreateCryptContext(
                  document.TrailerDictionary, userPassword));
            foreach (var item in document.Objects.Values)
            {
                if (!(await item.Target.DirectValueAsync()).ShouldWriteToFile()) continue;
                positions.DeclareIndirectObject(item.Target.ObjectNumber, target.BytesWritten);
                await DeclareContainedObjects(item, positions);
                await item.Target.Visit(objectWriter);
            }
            return positions;
        }

        private async Task DeclareContainedObjects(PdfIndirectReference item, XRefTable positions)
        {
            if (await item.DirectValueAsync() is IHasInternalIndirectObjects hiid)
            {
                int streamPosition = 0;
                foreach (var innerObjectNumber in await hiid.GetInternalObjectNumbersAsync())
                {
                    EnsureOuterGenerationNumberIsZero(item.Target);
                    positions.DeclareObjectStreamObject(
                        innerObjectNumber.ObjectNumber, item.Target.ObjectNumber, streamPosition++);
                }
            }
        }

        private void EnsureOuterGenerationNumberIsZero(PdfIndirectObject itemTarget)
        {
            if (itemTarget.GenerationNumber != 0)
                throw new InvalidOperationException("Object streams must hae a generation number of 0.");
        }

        private XRefTable CreateIndexArray(PdfLowLevelDocument document, int extraSlots)
        {
            var maxObject = document.Objects.Keys.Max(i => i.ObjectNumber);
            return new XRefTable(maxObject, extraSlots);
        }
    }
}