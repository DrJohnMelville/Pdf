using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public interface ILowLevelDocumentModifier
    {
    //    void DeleteObject(int objectNum);
    }

    public class LowLevelDocumentModifier : ILowLevelDocumentModifier
    {
        private readonly LowLevelDocumentBuilder builder;
        private readonly long priorXref;
        private readonly long freeLisHead;
        private readonly List<int> deletedItems = new();

        public LowLevelDocumentModifier(int nextObjectNum, long priorXref, long freeLisHead)
        {
            builder = new LowLevelDocumentBuilder(nextObjectNum);
            this.priorXref = priorXref;
            this.freeLisHead = freeLisHead;
        }

        public LowLevelDocumentModifier(PdfLoadedLowLevelDocument document) : 
            this(document.Objects.Keys.Max(i=>i.ObjectNumber), document.XRefPosition, document.FirstFreeBlock)
        {
            // foreach (var (key, value) in document.TrailerDictionary.RawItems)
            // {
            //     
            // }
       }

        public ValueTask<FlushResult> WriteModificationTrailer(PipeWriter cpw, long startPosition) =>
            WriteModificationTrailer(new CountingPipeWriter(cpw, startPosition));
        public ValueTask<FlushResult> WriteModificationTrailer(CountingPipeWriter target)
        {
            XrefTableElementWriter.WriteXrefTitleLine(target);
            return target.FlushAsync();
        }
    }
}