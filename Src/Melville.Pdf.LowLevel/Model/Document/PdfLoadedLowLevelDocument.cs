using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Document;

public sealed class PdfLoadedLowLevelDocument: PdfLowLevelDocument, IDisposable{
    public long XRefPosition { get; }
    public long FirstFreeBlock { get; }
    private readonly IDisposable fileOwner;

    public PdfLoadedLowLevelDocument(
        byte majorVersion, byte minorVersion, PdfDictionary trailerDictionary, 
        IReadOnlyDictionary<(int, int), PdfIndirectObject> objects, long xRefPosition, 
        long firstFreeBlock, IDisposable fileOwner ) : 
        base(majorVersion, minorVersion, trailerDictionary, objects)
    {
        this.fileOwner = fileOwner;
        XRefPosition = xRefPosition;
        FirstFreeBlock = firstFreeBlock;
    }

    public void Dispose() => fileOwner.Dispose();
}