using System.Threading.Tasks;
using Melville.CCITT;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittFilterFactory
{
    public static async ValueTask<IStreamFilterDefinition> Encoder(PdfObject? arg) => 
        CcittCodecFactory.SelectEncoder(await ParseCcittOptionsAsync(arg).CA());

    public static async ValueTask<IStreamFilterDefinition> Decoder(PdfObject? arg) => 
        CcittCodecFactory.SelectDecoder(await ParseCcittOptionsAsync(arg).CA());

    public static ValueTask<CcittParameters> ParseCcittOptionsAsync(PdfObject? parameters) =>
        FromDictionary(parameters as PdfDictionary ?? PdfDictionary.Empty);
    public static async ValueTask<CcittParameters> FromDictionary(PdfDictionary parameters) =>
        new((int)await parameters.GetOrDefaultAsync(KnownNames.K, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EncodedByteAlign, false).CA(),
            (int)await parameters.GetOrDefaultAsync(KnownNames.Columns, 1728).CA(),
            (int)await parameters.GetOrDefaultAsync(KnownNames.Rows, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EndOfBlock, true).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.BlackIs1, false).CA()
        );
}