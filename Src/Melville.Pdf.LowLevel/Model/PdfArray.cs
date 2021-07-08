using System.Collections.Generic;

namespace Melville.Pdf.LowLevel.Model
{
    public class PdfArray : PdfObject
    {
        public PdfObject[] RawItems { get; }

        public PdfArray(PdfObject[] rawItems)
        {
            RawItems = rawItems;
        }
    }

    public class PdfDictionary : PdfObject
    {
        public Dictionary<PdfName, PdfObject> RawItems;

        public PdfDictionary(Dictionary<PdfName, PdfObject> rawItems)
        {
            RawItems = rawItems;
        }
    }
}