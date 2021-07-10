using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Parsing.NameParsing;

namespace Melville.Pdf.LowLevel.Model
{
    public sealed partial class PdfDictionary : PdfObject, IReadOnlyDictionary<PdfName, PdfObject>
    {
        [DelegateTo()]
        public IReadOnlyDictionary<PdfName, PdfObject> RawItems { get; }

        public PdfDictionary(IReadOnlyDictionary<PdfName, PdfObject> rawItems)
        {
            RawItems = rawItems;
        }

        #region Dictionary Implementation

        public IEnumerator<KeyValuePair<PdfName, PdfObject>> GetEnumerator() =>
            RawItems
                .Select(i => new KeyValuePair<PdfName, PdfObject>(i.Key, i.Value.DirectValue()))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGetValue(PdfName key, [MaybeNullWhen(false)]out PdfObject value)
        {
            if (RawItems.TryGetValue(key, out var ret))
            {
                value = ret.DirectValue();
                return true;
            }

            value = null;
            return false;
        }

        public PdfObject this[PdfName key] => RawItems[key].DirectValue();
        
        public IEnumerable<PdfObject> Values => RawItems.Values.Select(i => i.DirectValue());

        #endregion


        #region Type and Subtype as definted in the standard 7.3.7

        public PdfName? Type => TryGetValue(KnownNames.Type, out var obj) ? obj as PdfName : null;
        public PdfName? SubType => TryGetValue(
            KnownNames.SubType, out var obj) || TryGetValue(KnownNames.S, out obj)? obj as PdfName : null;

        #endregion
    }
}