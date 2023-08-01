using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal static class ArrayParsingHelper
{
    public static async ValueTask<double[]?> ReadFixedLengthDoubleArrayAsync(this PdfValueDictionary dict, PdfDirectValue name, int length) =>
        (await dict.GetOrNullAsync<PdfValueArray>(name).CA()) is
        { } pdfArray && await pdfArray.CastAsync<double>().CA() is {} ret && ret.Length == length
            ? ret
            : null;

}