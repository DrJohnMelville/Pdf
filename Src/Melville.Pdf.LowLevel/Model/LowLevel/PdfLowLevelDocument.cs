namespace Melville.Pdf.LowLevel.Model.LowLevel
{
    public class PdfLowLevelDocument
    {
        public byte MajorVersion {get;}
        public byte MinorVersion {get;}

        public PdfLowLevelDocument(byte majorVersion, byte minorVersion)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }
    }
}