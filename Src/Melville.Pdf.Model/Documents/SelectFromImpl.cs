using System.Collections.Generic;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents
{
    /// <summary>
    /// Extension object to select items out of an enumerable of PdfDirectObjects
    /// </summary>
    public static class SelectFromImpl
    {
        /// <summary>
        /// Extract objects of a given type from an enumerable pf PdfDirectObjecs
        /// </summary>
        /// <typeparam name="T">The expected output type</typeparam>
        /// <param name="items">The enumerable of PdfObjects to check</param>
        /// <returns>The original enumerable, casted to a given type</returns>
        public static IEnumerable<T> SelectInner<T>(this IEnumerable<PdfDirectObject> items)
        {
            foreach (var item in items)
            {
                if (item.TryGet(out T? casted)) yield return casted;
            }
        }

        /// <summary>
        /// Extract objects of a given type from an enumerable pf PdfDirectObjecs
        /// </summary>
        /// <typeparam name="T">The expected output type</typeparam>
        /// <param name="items">The enumerable of PdfObjects to check</param>
        /// <returns>The original enumerable, casted to a given type</returns>
        public static async IAsyncEnumerable<T> SelectInnerAsync<T>(this IAsyncEnumerable<PdfDirectObject> items)
        {
            await foreach (var item in items.CA())
            {
                if (item.TryGet(out T? casted)) yield return casted;
            }
        }
    }
}