using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

/// <summary>
/// This class takes a low level document and writes it out to a PipeWriter in Pdf Format
/// </summary>
public class LowLevelDocumentWriter
{
    private readonly CountingPipeWriter target;
    /// <summary>
    /// The pipewriter to which the document is being written.
    /// </summary>
    protected PipeWriter Target => target;
    /// <summary>
    /// The document being written.
    /// </summary>
    protected readonly PdfLowLevelDocument document;
    private readonly string? userPassword;

    /// <summary>
    /// Create a LowLevelDocumentWriter
    /// </summary>
    /// <param name="target">The PipeWriter to receive the document</param>
    /// <param name="document">The document to write</param>
    /// <param name="userPassword">The user password with which to encrypt the doucment</param>
    public LowLevelDocumentWriter(
        PipeWriter target, PdfLowLevelDocument document, string? userPassword = null)
    {
        this.target = new CountingPipeWriter(target);
        this.document = document;
        this.userPassword = userPassword;
    }

    /// <summary>
    /// Write the document specified in the constructor to the pipe specified in the constructor.
    /// This method uses a classical XREF table to write the object references
    /// </summary>
    /// <returns></returns>
    public async Task WriteAsync()
    {
            
        var objectOffsets = await WriteHeaderAndObjectsAsync().CA();
        await WriteReferencesAndTrailerAsync(objectOffsets, target.BytesWritten).CA();
    }

    /// <summary>
    /// Number of blank slots to add to the end of the XRef table.  The XrefStreamLowLevelDocumentWriter
    /// uses this to reserve a final slot for the xrefstream.
    /// </summary>
    /// <returns>The number of slots to reserve</returns>
    private protected virtual int ExtraSlotsNeeded() => 0;

    /// <summary>
    /// Write the references and trailer.  There are 2 styles, classic Xref table and XrefStream.
    /// Different objects write the different styles
    /// </summary>
    /// <param name="objectOffsets">The table of object locations</param>
    /// <param name="xRefStart">Location within the stream of the current position</param>
    /// <returns></returns>
    private protected virtual async Task WriteReferencesAndTrailerAsync(XRefTable objectOffsets, long xRefStart)
    {
        await NewXrefTableWriter.WriteXrefsForNewFileAsync(Target, objectOffsets).CA();
        await TrailerWriter.WriteTrailerWithDictionaryAsync(Target, document.TrailerDictionary, xRefStart).CA();
    }

    private async Task<XRefTable> WriteHeaderAndObjectsAsync()
    {
        HeaderWriter.WriteHeader(Target, document.MajorVersion, document.MinorVersion);
        return await WriteObjectListAsync(document).CA();
    }

    private async Task<XRefTable> WriteObjectListAsync(PdfLowLevelDocument document)
    {
        var positions= CreateIndexArray(document);
        var objectWriter = new PdfObjectWriter(Target,
            await TrailerToDocumentCryptContext.CreateCryptContextAsync(
                document.TrailerDictionary, userPassword).CA());
        foreach (var item in document.Objects)
        {
            if (item.Value.IsNull) continue;
            positions.DeclareIndirectObject(item.Key.ObjectNumber, target.BytesWritten, item.Key.GenerationNumber);
            var value = await ResolveValueToWrite(item).CA();
            await DeclareContainedObjectsAsync(item.Key.ObjectNumber, value, positions).CA();
            await objectWriter.WriteTopLevelDeclarationAsync(
                item.Key.ObjectNumber, item.Key.GenerationNumber, value).CA();
            await Target.FlushAsync().CA();
        }
        return positions;
    }

    private static async Task<PdfDirectValue> ResolveValueToWrite(KeyValuePair<(int ObjectNumber, int GenerationNumber), PdfIndirectValue> item)
    {
        var value = await item.Value.LoadValueAsync().CA();
        if (value.TryGet(out ObjectStreamBuilder? osb))
        {
            value = await osb.CreateStreamAsync().CA();
        }

        return value;
    }

    private static async Task DeclareContainedObjectsAsync(int outerStreamNumber, PdfIndirectValue item,
        XRefTable positions)
    {
        if ((await item.LoadValueAsync().CA()).TryGet(out IHasInternalIndirectObjects? hiid))
        {
            int streamPosition = 0;
            foreach (var innerObjectNumber in await hiid.GetInternalObjectNumbersAsync().CA())
            {
                 positions.DeclareObjectStreamObject(
                    innerObjectNumber.ObjectNumber, outerStreamNumber, streamPosition++);
            }
        }
    }

    private XRefTable CreateIndexArray(PdfLowLevelDocument document)
    {
        var maxObject = document.Objects.Keys.Max(i => i.ObjectNumber);
        return new XRefTable(maxObject, ExtraSlotsNeeded());
    }
}