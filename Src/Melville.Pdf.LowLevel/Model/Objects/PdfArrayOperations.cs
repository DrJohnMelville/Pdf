using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Convenience methods for getting values out of a PdfArray.
/// </summary>
public static class PdfArrayOperations
{
    /// <summary>
    /// Convenience method to get a value from a PDF Array and cast it to a desired type.
    /// </summary>
    /// <typeparam name="T">The desired type</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="index">The index of the desired item</param>
    /// <returns>The indexed item, with indirects resolved, and then casted to the
    /// desired type</returns>
    public static async ValueTask<T> GetAsync<T>(this PdfArray array, int index) =>
        (await array[index].CA()).Get<T>();

}