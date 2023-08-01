using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.LowLevel.Model.Document;

/// <summary>
/// This represents a pdf document that was loaded from a stream.  It has some additional fields needed to write out a modified stream.
///
/// The class adds  nothing public to PdfLowLevelDocument, but users need to be able tor reference this type by name to pass it into
/// certian APIs that require a PDFLoadedLowLevelDocument.
/// </summary>
public sealed class PdfLoadedLowLevelDocument: PdfLowLevelDocument, IDisposable{
    internal long XRefPosition { get; }
    private readonly IDisposable fileOwner;

    internal PdfLoadedLowLevelDocument(
        byte majorVersion, byte minorVersion, PdfValueDictionary trailerDictionary, 
        IReadOnlyDictionary<(int, int), PdfIndirectValue> objects, long xRefPosition, 
        IDisposable fileOwner ) : 
        base(majorVersion, minorVersion, trailerDictionary, objects)
    {
        this.fileOwner = fileOwner;
        XRefPosition = xRefPosition;
    }

    /// <inheritdoc />
    public void Dispose() => fileOwner.Dispose();
}