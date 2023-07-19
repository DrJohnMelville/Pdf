using System.Threading.Tasks;
using Melville.CCITT;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

internal class CcittFilterFactory
{
    public static async ValueTask<IStreamFilterDefinition> EncoderAsync(PdfDirectValue arg) => 
        CcittCodecFactory.SelectEncoder(await ParseCcittOptionsAsync(arg).CA());

    public static async ValueTask<IStreamFilterDefinition> DecoderAsync(PdfDirectValue arg) => 
        CcittCodecFactory.SelectDecoder(await ParseCcittOptionsAsync(arg).CA());

    public static ValueTask<CcittParameters> ParseCcittOptionsAsync(PdfDirectValue parameters) =>
        FromDictionaryAsync(parameters.TryGet(out PdfValueDictionary dict)? dict:PdfValueDictionary.Empty);

    public static async ValueTask<CcittParameters> FromDictionaryAsync(PdfValueDictionary parameters) =>
        new(await parameters.GetOrDefaultAsync(KnownNames.KTName, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EncodedByteAlignTName, false).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.ColumnsTName, 1728).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.RowsTName, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EndOfBlockTName, true).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.BlackIs1TName, false).CA()
        );
}