using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Document
{
    public class PdfLoadedLowLevelDocument: PdfLowLevelDocument{
        public long XRefPosition { get; }

        public PdfLoadedLowLevelDocument(
            byte majorVersion, byte minorVersion, PdfDictionary trailerDictionary, 
            IReadOnlyDictionary<(int, int), PdfIndirectReference> objects, long xRefPosition) : 
            base(majorVersion, minorVersion, trailerDictionary, objects)
        {
            XRefPosition = xRefPosition;
        }
    }
}