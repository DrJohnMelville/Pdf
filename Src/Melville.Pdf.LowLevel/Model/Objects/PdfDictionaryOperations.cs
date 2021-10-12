﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public static class PdfDictionaryOperations
    {
        public static async ValueTask<T> GetAsync<T>(this PdfDictionary source, PdfName key)
        {
            if (source.TryGetValue(key, out var obj) && await obj is T ret) return ret;
            throw new PdfParseException($"Item {key} is not in dictionary or is wrong type");
        }

        public static async ValueTask<PdfObject> GetOrNullAsync(this PdfDictionary dict, PdfName name) =>
            dict.TryGetValue(name, out var obj) && 
            await obj is {} definiteObj? definiteObj: PdfTokenValues.Null;
        public static async ValueTask<long> GetOrDefaultAsync(
            this PdfDictionary dict, PdfName name, long defaultValue) =>
            dict.TryGetValue(name, out var obj) && 
            await obj is PdfNumber definiteObj? definiteObj.IntValue: defaultValue;
        public static async ValueTask<T> GetOrDefaultAsync<T>(
            this PdfDictionary dict, PdfName name, T defaultValue) where T:PdfObject =>
            dict.TryGetValue(name, out var obj) && 
            await obj is T definiteObj? definiteObj: defaultValue;

        public static IReadOnlyDictionary<PdfName, PdfObject> MergeItems(this PdfDictionary source, params (PdfName, PdfObject)[] items)
        {
            var ret = new Dictionary<PdfName, PdfObject>();
            foreach (var pair in source.RawItems)
            {
                ret[pair.Key] = pair.Value;
            }

            foreach (var (key, value) in items)
            {
                ret[key] = value;
            }

            return ret;
        }
        
        public static IEnumerable<(PdfName Name, PdfObject Value)> StripTrivialItems(
            this IEnumerable<(PdfName Name, PdfObject Value)> items) =>
            items.Where(NotAnEmptyObject);

        private static bool NotAnEmptyObject((PdfName Name, PdfObject Value) arg) =>
            !(arg.Value == PdfTokenValues.Null ||
              arg.Value is PdfArray { Count: 0 } ||
              arg.Value is PdfDictionary { Count: 0 });
    }
}