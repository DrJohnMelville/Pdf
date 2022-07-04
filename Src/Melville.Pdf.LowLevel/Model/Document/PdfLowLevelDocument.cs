using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Document;

public partial class PdfLowLevelDocument
{
    [FromConstructor]public byte MajorVersion {get;}
    [FromConstructor]public byte MinorVersion {get;}
    [FromConstructor]public PdfDictionary TrailerDictionary { get; }
    [FromConstructor]public IReadOnlyDictionary<(int ObjectNumber,int GenerationNumber), PdfIndirectObject> 
        Objects { get; }
    
    public void VerifyCanSupportObjectStreams()
    {
        if (MajorVersion < 2 && MinorVersion < 5)
            throw new PdfParseException("Object streams unavailable before pdf version 1.5");   

    }
}