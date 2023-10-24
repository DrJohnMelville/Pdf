using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Model.Conventions;
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
            RemoveReferenceStream(trailerDictionary ?? document.TrailerDictionary), 
            WrapDictionary(document.Objects))
    {
    }

    private static PdfDictionary RemoveReferenceStream(PdfDictionary trailer)
    {
        return trailer is PdfStream ? 
            new DictionaryBuilder(trailer.RawItems.Where(i => !IsRefStreamKey(i.Key))).AsDictionary() : 
            trailer;
    }

    private static bool IsRefStreamKey(PdfDirectObject key) =>
        key.Equals(KnownNames.Type) ||
        key.Equals(KnownNames.Size) ||
        key.Equals(KnownNames.Index) ||
        key.Equals(KnownNames.Prev) ||
        key.Equals(KnownNames.DecodeParms) ||
        key.Equals(KnownNames.Filter) ||
        key.Equals(KnownNames.Length) ||
        key.Equals(KnownNames.W);

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