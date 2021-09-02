using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            RawItems[key].DirectValue();

        public IEnumerable<PdfName> Keys => RawItems.Keys;

        IEnumerable<ValueTask<PdfObject>> IReadOnlyDictionary<PdfName, ValueTask<PdfObject>>.Values =>
            RawItems.Values.Select(i => i.DirectValue());

        public IEnumerator<KeyValuePair<PdfName, ValueTask<PdfObject>>> GetEnumerator() =>
            RawItems
                .Select(i => new KeyValuePair<PdfName, ValueTask<PdfObject>>(i.Key, i.Value.DirectValue()))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public bool TryGetValue(PdfName key, out ValueTask<PdfObject> value)
        {
            if (RawItems.TryGetValue(key, out var ret))
            {
                value = ret.DirectValue();
                return true;
            }
            value = default;
            return false;
        }

        public ValueTask<PdfObject> this[PdfName key] => RawItems[key].DirectValue();
        
        public IEnumerable<ValueTask<PdfObject>> Values => RawItems.Values.Select(i => i.DirectValue());

        #endregion


        #region Type and Subtype as definted in the standard 7.3.7

        public PdfName? Type => RawItems.TryGetValue(KnownNames.Type, out var obj) ? obj as PdfName : null;
        public PdfName? SubType => RawItems.TryGetValue(KnownNames.Subtype, out var obj) || 
                                   RawItems.TryGetValue(KnownNames.S, out obj)? obj as PdfName : null;

        #endregion
        
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    }

    public static class PdfDictionaryOperations
    {
        public static async ValueTask<T> GetAsync<T>(this PdfDictionary source, PdfName key)
        {
            if (source.TryGetValue(key, out var obj) && await obj is T ret) return ret;
            throw new ArgumentException("Expected item is not in dictionary or is wrong type");
        }

        public static async ValueTask<PdfObject> GetOrNullAsync(this PdfDictionary dict, PdfName name) =>
            dict.TryGetValue(name, out var obj) && 
            await obj is {} definiteObj? definiteObj: PdfTokenValues.Null;
        public static async ValueTask<long> GetOrDefaultAsync(
            this PdfDictionary dict, PdfName name, long defaultValue) =>
            dict.TryGetValue(name, out var obj) && 
            await obj is PdfNumber definiteObj? definiteObj.IntValue: defaultValue;
    }
 }