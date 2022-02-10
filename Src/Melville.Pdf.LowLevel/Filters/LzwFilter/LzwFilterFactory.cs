using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

public static class LzwFilterFactory
{
    public static async ValueTask<IStreamFilterDefinition> Decoder(PdfObject? p) => 
        new LzwDecodeFilter(await p.EarlySwitchLength().CA());

    public static async ValueTask<IStreamFilterDefinition> Encoder(PdfObject? p) => 
        new LzwEncodeFilter(await p.EarlySwitchLength().CA());
}