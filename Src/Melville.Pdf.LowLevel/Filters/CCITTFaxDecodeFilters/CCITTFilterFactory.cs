using System.Threading.Tasks;
using Melville.CCITT;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

internal class CcittFilterFactory
{
    public static async ValueTask<IStreamFilterDefinition> EncoderAsync(PdfDirectObject arg) => 
        CcittCodecFactory.SelectEncoder(await ParseCcittOptionsAsync(arg).CA());

    public static async ValueTask<IStreamFilterDefinition> DecoderAsync(PdfDirectObject arg) => 
        CcittCodecFactory.SelectDecoder(await ParseCcittOptionsAsync(arg).CA());

    public static ValueTask<CcittParameters> ParseCcittOptionsAsync(PdfDirectObject parameters) =>
        FromDictionaryAsync(parameters.TryGet(out PdfDictionary dict)? dict:PdfDictionary.Empty);

    public static async ValueTask<CcittParameters> FromDictionaryAsync(PdfDictionary parameters) =>
        new(await parameters.GetOrDefaultAsync(KnownNames.K, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EncodedByteAlign, false).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.Columns, 1728).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.Rows, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EndOfBlock, true).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.BlackIs1, false).CA()
        );
}