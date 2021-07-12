using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.LowLevel
{
    public class PdfLowLevelDocument
    {
        public byte MajorVersion {get;}
        public byte MinorVersion {get;}
        public PdfDictionary TrailerDictionary { get; }

        public PdfLowLevelDocument(byte majorVersion, byte minorVersion, PdfDictionary trailerDictionary)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            TrailerDictionary = trailerDictionary;
        }
    }
}