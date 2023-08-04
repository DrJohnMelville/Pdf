using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal static class ArrayParsingHelper
{
    public static async ValueTask<IReadOnlyList<double>?> ReadFixedLengthDoubleArrayAsync(this PdfDictionary dict, PdfDirectObject name, int length) =>
        (await dict.GetOrNullAsync<PdfArray>(name).CA()) is
        { } pdfArray && await pdfArray.CastAsync<double>().CA() is {} ret && ret.Count == length
            ? ret
            : null;

}