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
            HeaderWriter.WriteHeader(target, document.MajorVersion, document.MinorVersion);
            await target.FlushAsync();
            var objectOffsets = await WriteObjectList(document);
            long xRefStart = target.BytesWritten;
            await NewXrefTableWriter.WriteXrefsForNewFile(target, objectOffsets);
            await TrailerWriter.WriteTrailer(target, document.TrailerDictionary, xRefStart);
        }

        private async Task<long[]> WriteObjectList(PdfLowLevelDocument document)
        {
            var positions= CreateIndexArray(document);
            var objectWriter = new PdfObjectWriter(target);
            foreach (var item in document.Objects.Values)
            {
                positions[item.Target.ObjectNumber] = target.BytesWritten;
                await item.Target.Visit(objectWriter);
            }
            return positions;
        }

        private  long[] CreateIndexArray(PdfLowLevelDocument document)
        {
            var maxObject = document.Objects.Keys.Max(i => i.ObjectNumber);
            return new long[maxObject + 1];
        }
    }
}