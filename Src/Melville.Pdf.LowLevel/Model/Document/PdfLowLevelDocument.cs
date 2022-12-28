using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Document;

/// <summary>
/// A PdfLowlevelDocument is a Dictionary of objects the key of which is a pair of Object and Generation numbers.  The document also
/// has a version number (with major and minor parts) and a trailer dictionary that is used as a root to the remainder of the objects
/// in the document.
/// </summary>
public partial class PdfLowLevelDocument
{
    /// <summary>
    /// The major portion of the version number.
    /// </summary>
    [FromConstructor]public byte MajorVersion {get;}
    /// <summary>
    /// The minor portion of the version number
    /// </summary>
    [FromConstructor]public byte MinorVersion {get;}
    /// <summary>
    /// Trailer dictionary, which contains pointers to various important objects like encryption information and the document root.
    /// </summary>
    [FromConstructor]public PdfDictionary TrailerDictionary { get; }
    /// <summary>
    /// A dictionary of the PDF objects (other than the trailer dictionary) that comprise the low level document.
    /// </summary>
    [FromConstructor]public IReadOnlyDictionary<(int ObjectNumber,int GenerationNumber), PdfIndirectObject> 
        Objects { get; }
}