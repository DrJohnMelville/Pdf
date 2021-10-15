using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public partial class PdfDictionary : PdfObject, IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>
    {
        public IReadOnlyDictionary<PdfName, PdfObject> RawItems { get; }

        public PdfDictionary(IReadOnlyDictionary<PdfName, PdfObject> rawItems)
        {
            RawItems = rawItems;
        }

        public PdfDictionary(params (PdfName, PdfObject)[] items) : this(PairsToDictionary(items))
        {
        }

        public PdfDictionary(IEnumerable<(PdfName, PdfObject)> items): this(PairsToDictionary(items))
        {
        }

        protected static Dictionary<PdfName, PdfObject> PairsToDictionary(
            IEnumerable<(PdfName Name, PdfObject Value)> items) =>
            new(
                items.Select(i => new KeyValuePair<PdfName, PdfObject>(i.Name, i.Value)));


        #region Dictionary Implementation

        public int Count => RawItems.Count;

        public bool ContainsKey(PdfName key) => RawItems.ContainsKey(key);

        ValueTask<PdfObject> IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>.this[PdfName key] =>
            RawItems[key].DirectValueAsync();

        public IEnumerable<PdfName> Keys => RawItems.Keys;

        IEnumerable<ValueTask<PdfObject>> IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>.Values =>
            RawItems.Values.Select(i => i.DirectValueAsync());

        public IEnumerator<KeyValuePair<PdfName, ValueTask<PdfObject>>> GetEnumerator() =>
            RawItems
                .Select(i => new KeyValuePair<PdfName, ValueTask<PdfObject>>(i.Key, i.Value.DirectValueAsync()))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public bool TryGetValue(PdfName key, out ValueTask<PdfObject> value)
        {
            if (RawItems.TryGetValue(key, out var ret))
            {
                value = ret.DirectValueAsync();
                return true;
            }
            value = default;
            return false;
        }

        public ValueTask<PdfObject> this[PdfName key] => RawItems[key].DirectValueAsync();
        
        public IEnumerable<ValueTask<PdfObject>> Values => RawItems.Values.Select(i => i.DirectValueAsync());

        #endregion


        #region Type and Subtype as definted in the standard 7.3.7

        public PdfName? Type => RawItems.TryGetValue(KnownNames.Type, out var obj) ? obj as PdfName : null;
        #warning  make KnownName.S alias to KnownNames.SubType
        public PdfName? SubType => RawItems.TryGetValue(KnownNames.Subtype, out var obj) || 
                                   RawItems.TryGetValue(KnownNames.S, out obj)? obj as PdfName : null;

        #endregion
        
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    }
}