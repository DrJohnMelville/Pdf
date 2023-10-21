using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// A modifiable Low Level Document
/// </summary>
public class ModifyableLowLevelDocument: PdfLowLevelDocument, ICanReplaceObjects
{
    /// <summary>
    /// Create a ModifiableLowLevelDocument from a source document
    /// </summary>
    /// <param name="document">The source document</param>
    /// <param name="majorVersion">The major version of the document.</param>
    /// <param name="minorVersion">The minor version of the document</param>
    /// <param name="trailerDictionary">The trailer dictionary</param>
    public ModifyableLowLevelDocument(PdfLowLevelDocument document, byte majorVersion = 255, byte minorVersion=255, PdfDictionary? trailerDictionary = null) : 
        base(majorVersion is 255 ? document.MajorVersion:majorVersion, 
            minorVersion is 255 ? document.MinorVersion: minorVersion, 
            trailerDictionary ?? document.TrailerDictionary, WrapDictionary(document.Objects))
    {
    }

    private static IReadOnlyDictionary<(int ObjectNumber, int GenerationNumber), PdfIndirectObject> 
        WrapDictionary(IReadOnlyDictionary<(int ObjectNumber, int GenerationNumber), PdfIndirectObject> documentObjects) => 
        new Dictionary<(int ObjectNumber, int GenerationNumber), PdfIndirectObject>(documentObjects);

    /// <inheritdoc />
    public void ReplaceReferenceObject(PdfIndirectObject reference, PdfDirectObject value)
    {
        ((Dictionary<(int ObjectNumber, int GenerationNumber), PdfIndirectObject>)Objects)
            [reference.GetObjectReference()] = value;
    }
}