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

/// <summary>
/// This interface allows additions and replacement of objects in a PdfLowLevelDocument
/// Then a follow up trailer can be written that modifies a PDF document using the new or replaced objects.
/// </summary>
public interface ILowLevelDocumentModifier : IPdfObjectRegistry
{
    /// <summary>
    /// Give an indirect object reference a new value
    /// </summary>
    /// <param name="reference">The reference to reassign</param>
    /// <param name="value">The new value</param>
    void ReplaceReferenceObject(PdfIndirectObject reference, PdfObject value);
    /// <summary>
    /// Append the modification trailer to a stream.
    /// </summary>
    /// <param name="stream">The stream to be appended to.</param>
    /// <returns>The valuetask that monitors the completion.</returns>
    ValueTask WriteModificationTrailerAsync(Stream stream) =>
        WriteModificationTrailerAsync(PipeWriter.Create(stream), stream.Position);
    /// <summary>
    /// Writes out a modification trailer to a PipeWriter.
    /// </summary>
    /// <param name="cpw">The pipe writer to write to</param>
    /// <param name="startPosition">The index at which the trailer starts.</param>
    /// <returns>ValueTask controlling the completion of the operation.</returns>
    ValueTask WriteModificationTrailerAsync(PipeWriter cpw, long startPosition);

}

internal partial class LowLevelDocumentModifier : ILowLevelDocumentModifier
{
    private readonly PdfObjectRegistry builder;
    [DelegateTo(Visibility = SourceLocationVisibility.Public)]
    private IPdfObjectRegistry InnerBuilder => builder;
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
    
    public void ReplaceReferenceObject(PdfIndirectObject reference, PdfObject value)
    {
            builder.Add(value, reference.ObjectNumber, reference.GenerationNumber);
    }


    public ValueTask WriteModificationTrailerAsync(PipeWriter cpw, long startPosition) =>
        WriteModificationTrailerAsync(new CountingPipeWriter(cpw, startPosition));
    private async ValueTask WriteModificationTrailerAsync(CountingPipeWriter target)
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
        await TrailerWriter.WriteTrailerWithDictionaryAsync(target, builder.CreateTrailerDictionary(), startXref).CA();
    }

    private static void WriteRevisedXrefTable(CountingPipeWriter target, IEnumerable<XrefLine> lines)
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

    private static IEnumerable<IList<XrefLine>> XRefSegments(IEnumerable<XrefLine> lines)
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

internal readonly partial struct XrefLine
{
    [FromConstructor] public int ObjectNumber { get; }
    [FromConstructor] public long Offset { get; }
    [FromConstructor] public int Generation { get; }
    [FromConstructor] public bool Used { get; }
}