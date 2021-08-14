using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public static class LzwParameterParser
    {
        public static async ValueTask<int> EarlySwitchLength(this PdfObject? parameters)
        {
            return (parameters is PdfDictionary dict &&
                    dict.TryGetValue(KnownNames.EarlyChange, out var ec) &&
                    await ec is PdfNumber num)
                ? (int)num.IntValue
                : 1;
        }

    }
}