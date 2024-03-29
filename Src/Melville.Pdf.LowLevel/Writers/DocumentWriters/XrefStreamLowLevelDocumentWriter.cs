﻿using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Document;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

/// <summary>
/// Writes a low level document to a stream using XRefStreams to locate objects.
/// </summary>
public class XrefStreamLowLevelDocumentWriter: LowLevelDocumentWriter
{
    /// <inheritdoc />
    public XrefStreamLowLevelDocumentWriter(PipeWriter target, PdfLowLevelDocument document, string? userPassword = null) :
        base(target, document, userPassword)
    {
        document.VerifyCanSupportObjectStreams();
    }

    /// <inheritdoc />
    private protected override async Task WriteReferencesAndTrailerAsync(XRefTable objectOffsets, long xRefStart)
    {
        await new ReferenceStreamWriter(Target, document, objectOffsets).WriteAsync().CA();
        await TrailerWriter.WriteTerminalStartXrefAndEofAsync(Target, xRefStart).CA();
    }

    /// <inheritdoc />
    private protected override int ExtraSlotsNeeded() => 1;
}