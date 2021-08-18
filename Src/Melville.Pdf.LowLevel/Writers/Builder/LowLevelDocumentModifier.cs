using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public interface ILowLevelDocumentModifier: ILowLevelDocumentBuilder
    {
        void DeleteObject(PdfIndirectObject objectNum);
    }

    public static class LowLevelDocumentModifierOperations
    {
        public static void DeleteObject(this ILowLevelDocumentModifier mod, PdfIndirectReference item) =>
            mod.DeleteObject(item.Target);
    }

    public partial class LowLevelDocumentModifier : ILowLevelDocumentModifier
    {
        private readonly LowLevelDocumentBuilder builder;
        [DelegateTo()]
        private ILowLevelDocumentBuilder innerBuilder => builder;
        private readonly long priorXref;
        private readonly long freeLisHead;
        private readonly List<PdfIndirectObject> deletedItems = new();

        public LowLevelDocumentModifier(int nextObjectNum, long priorXref, long freeLisHead)
        {
            builder = new LowLevelDocumentBuilder(nextObjectNum);
            this.priorXref = priorXref;
            this.freeLisHead = freeLisHead;
        }

        public void DeleteObject(PdfIndirectObject objectNum) => deletedItems.Add(objectNum);
        public void AssignValueToReference(PdfIndirectReference reference, PdfObject value)
        {
            if (builder.Objects.Contains(reference))
            {
                builder.AssignValueToReference(reference, value);
            }else if (builder.Objects.FirstOrDefault(
                i => IsSameObject(i.Target, reference.Target)) is { } newObj)
            {
                builder.AssignValueToReference(newObj, value);
            }
            else
            {
                builder.Add(value, reference.Target.ObjectNumber, reference.Target.GenerationNumber);
            }
        }

        private bool IsSameObject(PdfIndirectObject a, PdfIndirectObject b) =>
            a.ObjectNumber == b.ObjectNumber && a.GenerationNumber == b.GenerationNumber;


        public LowLevelDocumentModifier(PdfLoadedLowLevelDocument document) : 
            this(document.Objects.Keys.Max(i=>i.ObjectNumber), document.XRefPosition, document.FirstFreeBlock)
        {
            foreach (var (key, value) in document.TrailerDictionary.RawItems)
            {
                builder.AddToTrailerDictionary(key, value);
            }
       }

        
        public ValueTask WriteModificationTrailer(Stream stream) =>
            WriteModificationTrailer(PipeWriter.Create(stream), stream.Position);
        public ValueTask WriteModificationTrailer(PipeWriter cpw, long startPosition) =>
            WriteModificationTrailer(new CountingPipeWriter(cpw, startPosition));
        public async ValueTask WriteModificationTrailer(CountingPipeWriter target)
        {
            var lines = new List<XrefLine>();
            var writer = new PdfObjectWriter(target);
            foreach (var item in builder.Objects)
            {
              lines.Add(new XrefLine(
                  item.Target.ObjectNumber, target.BytesWritten, item.Target.GenerationNumber, true));
              await writer.Visit(item.Target);
            }
            var startXref = target.BytesWritten;
            XrefTableElementWriter.WriteXrefTitleLine(target);
            DeletedItemLines(lines);
            WriteRevisedXrefTable(target, lines);
            await target.FlushAsync();
            builder.AddToTrailerDictionary(KnownNames.Prev, new PdfInteger(priorXref));
            await TrailerWriter.WriteTrailer(target, builder.CreateTrailerDictionary(), startXref);
        }

        private void WriteRevisedXrefTable(CountingPipeWriter target, IEnumerable<XrefLine> lines)
        {
            foreach (var segment in XRefSegments(lines))
            {
                XrefTableElementWriter.WriteTableHeader(target, FirstObjectNumber(segment), segment.Count);
                foreach (var line in segment)
                {
                    XrefTableElementWriter.WriteTableEntry(target, line.Offset, line.Generation, line.Used);
                }
            }
        }

        //If we are writing an empty xref table we still need a 0 0 header for a single empty block.
        private static int FirstObjectNumber(IList<XrefLine> segment) => 
            segment.Count > 0?segment[0].ObjectNumber:0;

        private void DeletedItemLines(IList<XrefLine> lines)
        {
            if (deletedItems.Count == 0) return;
            var currentFreeHead = freeLisHead;
            foreach (var i in deletedItems)
            {
                lines.Add(new XrefLine(i.ObjectNumber, currentFreeHead, i.GenerationNumber, false));
                currentFreeHead = i.ObjectNumber;
            }
            lines.Add(new XrefLine(0, currentFreeHead, 65535, false));
        }

        private IEnumerable<IList<XrefLine>> XRefSegments(IEnumerable<XrefLine> lines)
        {
            var ret = new List<XrefLine>();
            int last = int.MinValue;
            foreach (var line in lines.OrderBy(i=>i.ObjectNumber))
            {
                if (last >= 0 && last + 1 != line.ObjectNumber)
                {
                    yield return ret;
                    ret = new List<XrefLine>();
                }
                ret.Add(line);
                last = line.ObjectNumber;
            }
            yield return ret;
        }
    }

    public readonly struct XrefLine
    {
        public int ObjectNumber { get; }
        public long Offset { get; }
        public int Generation { get; }
        public bool Used { get; }

        public XrefLine(int objectNumber, long offset, int generation, bool used)
        {
            ObjectNumber = objectNumber;
            Offset = offset;
            Generation = generation;
            Used = used;
        }
    }
}