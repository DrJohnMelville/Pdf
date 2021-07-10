using System.Collections;
using System.Collections.Generic;

namespace Melville.Pdf.LowLevel.Model
{
    public sealed class PdfDictionary : PdfObject, IReadOnlyDictionary<PdfName, PdfObject>
    {
        public IReadOnlyDictionary<PdfName, PdfObject> RawItems { get; }

        public PdfDictionary(IReadOnlyDictionary<PdfName, PdfObject> rawItems)
        {
            RawItems = rawItems;
        }

        public IEnumerator<KeyValuePair<PdfName, PdfObject>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => RawItems.Count;

        public bool ContainsKey(PdfName key) => RawItems.ContainsKey(key);

        public bool TryGetValue(PdfName key, out PdfObject value)
        {
            throw new System.NotImplementedException();
        }

        public PdfObject this[PdfName key] => throw new System.NotImplementedException();

        public IEnumerable<PdfName> Keys => throw new System.NotImplementedException();

        public IEnumerable<PdfObject> Values => throw new System.NotImplementedException();
    }
}