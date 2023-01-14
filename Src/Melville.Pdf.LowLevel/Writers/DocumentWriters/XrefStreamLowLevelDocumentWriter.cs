using System.IO.Pipelines;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Document;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

public class XrefStreamLowLevelDocumentWriter: LowLevelDocumentWriter
{
    /// <inheritdoc />
    public XrefStreamLowLevelDocumentWriter(PipeWriter target, PdfLowLevelDocument document, string? userPassword = null) :
        base(target, document, userPassword)
    {
        document.VerifyCanSupportObjectStreams();
    }

    /// <inheritdoc />
    private protected override async Task WriteReferencesAndTrailer(XRefTable objectOffsets, long xRefStart)
    {
        await new ReferenceStreamWriter(target, document, objectOffsets).Write().CA();
        await TrailerWriter.WriteTerminalStartXrefAndEof(target, xRefStart).CA();
    }

    /// <inheritdoc />
    private protected override int ExtraSlotsNeeded() => 1;
}