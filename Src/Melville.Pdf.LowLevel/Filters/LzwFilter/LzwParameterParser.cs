using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public static class LzwParameterParser
    {
        public static async ValueTask<int> EarlySwitchLength(this PdfObject? parameters) =>
            parameters is PdfDictionary dict?
                (int)await dict.GetOrDefaultAsync(KnownNames.EarlyChange, 1): 1;
    }
}