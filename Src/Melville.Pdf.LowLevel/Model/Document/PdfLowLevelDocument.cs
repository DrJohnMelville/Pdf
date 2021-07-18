using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Document
{
    public class PdfLowLevelDocument
    {
        public byte MajorVersion {get;}
        public byte MinorVersion {get;}
        public PdfDictionary TrailerDictionary { get; }
        public IReadOnlyDictionary<(int ObjectNumber,int GenerationNumber), PdfIndirectReference> 
            Objects { get; }

        public PdfLowLevelDocument(
            byte majorVersion, byte minorVersion, PdfDictionary trailerDictionary, 
            IReadOnlyDictionary<(int, int), PdfIndirectReference> objects)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            TrailerDictionary = trailerDictionary;
            Objects = objects;
        }
    }
}