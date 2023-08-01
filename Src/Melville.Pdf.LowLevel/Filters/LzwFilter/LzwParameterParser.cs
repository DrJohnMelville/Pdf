using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

internal static class LzwParameterParser
{
    public static async ValueTask<int> EarlySwitchLengthAsync(this PdfDirectValue parameters) =>
        parameters.TryGet(out PdfValueDictionary dict)?
            await dict.GetOrDefaultAsync(KnownNames.EarlyChangeTName, 1).CA(): 1;
}