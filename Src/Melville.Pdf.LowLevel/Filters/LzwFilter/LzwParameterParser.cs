using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

internal static class LzwParameterParser
{
    public static async ValueTask<int> EarlySwitchLengthAsync(this PdfDirectObject parameters) =>
        parameters.TryGet(out PdfDictionary dict)?
            await dict.GetOrDefaultAsync(KnownNames.EarlyChange, 1).CA(): 1;
}