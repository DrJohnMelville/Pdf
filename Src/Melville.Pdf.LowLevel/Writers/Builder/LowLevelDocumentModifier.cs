using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public interface ILowLevelDocumentModifier : IPdfObjectRegistry
{
    void AssignValueToReference(PdfIndirectObject reference, PdfObject value);
    ValueTask WriteModificationTrailer(Stream stream) =>
        WriteModificationTrailer(PipeWriter.Create(stream), stream.Position);

    ValueTask WriteModificationTrailer(PipeWriter cpw, long startPosition);

}

internal partial class LowLevelDocumentModifier : ILowLevelDocumentModifier
{
    private readonly PdfObjectRegistry builder;
    [DelegateTo()]
    private IPdfObjectRegistry innerBuilder => builder;
    private readonly long priorXref;

    public LowLevelDocumentModifier(int nextObjectNum, long priorXref)
    {
        builder = new PdfObjectRegistry(nextObjectNum);
        this.priorXref = priorXref;
    }

    public LowLevelDocumentModifier(PdfLoadedLowLevelDocument document) :
        this(document.Objects.Keys.Max(i => i.ObjectNumber), document.XRefPosition)
    {
        foreach (var (key, value) in document.TrailerDictionary.RawItems)
        {
            builder.AddToTrailerDictionary(key, value);
        }
    }
    
    public void AssignValueToReference(PdfIndirectObject reference, PdfObject value)
    {
        if (builder.Objects.Contains(reference))
        {
            builder.AssignValueToReference(reference, value);
        }else if (builder.Objects.FirstOrDefault(
                      i => IsSameObject(i, reference)) is { } newObj)
        {
            builder.AssignValueToReference(newObj, value);
        }
        else
        {
            builder.Add(value, reference.ObjectNumber, reference.GenerationNumber);
        }
    }

    private bool IsSameObject(PdfIndirectObject a, PdfIndirectObject b) =>
        a.ObjectNumber == b.ObjectNumber && a.GenerationNumber == b.GenerationNumber;


    public ValueTask WriteModificationTrailer(PipeWriter cpw, long startPosition) =>
        WriteModificationTrailer(new CountingPipeWriter(cpw, startPosition));
    private async ValueTask WriteModificationTrailer(CountingPipeWriter target)
    {
        var lines = new List<XrefLine>();
        var writer = new PdfObjectWriter(target);
        foreach (var item in builder.Objects)
        {
            lines.Add(new XrefLine(
                item.ObjectNumber, target.BytesWritten, item.GenerationNumber, true));
            await writer.VisitTopLevelObject(item).CA();
        }
        var startXref = target.BytesWritten;
        XrefTableElementWriter.WriteXrefTitleLine(target);
        WriteRevisedXrefTable(target, lines);
        await target.FlushAsync().CA();
        builder.AddToTrailerDictionary(KnownNames.Prev, priorXref);
        await TrailerWriter.WriteTrailerWithDictionary(target, builder.CreateTrailerDictionary(), startXref).CA();
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

public readonly partial struct XrefLine
{
    [FromConstructor] public int ObjectNumber { get; }
    [FromConstructor] public long Offset { get; }
    [FromConstructor] public int Generation { get; }
    [FromConstructor] public bool Used { get; }
}