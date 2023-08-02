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
        new(await parameters.GetOrDefaultAsync(KnownNames.KTName, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EncodedByteAlignTName, false).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.ColumnsTName, 1728).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.RowsTName, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EndOfBlockTName, true).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.BlackIs1TName, false).CA()
        );
}