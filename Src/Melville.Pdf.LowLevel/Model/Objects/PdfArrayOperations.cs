using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Convenience methods for getting values out of a PdfArray.
/// </summary>
public static class PdfArrayOperations
{
    public static async ValueTask<T> GetAsync<T>(this PdfArray array, int index) =>
        (await array[index].CA()).Get<T>();

}