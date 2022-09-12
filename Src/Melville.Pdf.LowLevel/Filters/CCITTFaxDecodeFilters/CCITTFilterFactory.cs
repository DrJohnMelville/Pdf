using System.Threading.Tasks;
using Melville.CCITT;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class CcittFilterFactory
{
    public static async ValueTask<IStreamFilterDefinition> Encoder(PdfObject? arg) => SelectEncoderType(
        await ParseCcittOptionsAsync(arg).CA());

    private static IStreamFilterDefinition SelectEncoderType(CcittParameters args) => args.K switch
    {
        < 0 => new CcittType4Encoder(args),
        0 => new CcittType31dEncoder(args),
        > 0 => new CcittType3SwitchingEncoder(args)
    };

    public static async ValueTask<IStreamFilterDefinition> Decoder(PdfObject? arg) => 
        SelectDecoderType(await ParseCcittOptionsAsync(arg).CA());

    private static IStreamFilterDefinition SelectDecoderType(CcittParameters args) => args.K switch
    {
        < 0 => new CcittType4Decoder(args, new TwoDimensionalLineCodeDictionary()),
        0 => new CcittType4Decoder(args, new Type3K0LineCodeDictionary()),
        > 0 => new CcittType4Decoder(args, new Type3SwitchingLineCodeDictionary())
    };

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