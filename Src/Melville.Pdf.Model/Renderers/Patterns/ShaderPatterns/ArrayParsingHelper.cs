using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal static class ArrayParsingHelper
{
    public static async ValueTask<double[]?> ReadFixedLengthDoubleArrayAsync(this PdfDictionary dict, PdfName name, int length) =>
        (await dict.GetOrNullAsync<PdfArray>(name).CA()) is
        { } pdfArray && await pdfArray.AsDoublesAsync().CA() is {} ret && ret.Length == length
            ? ret
            : null;

}