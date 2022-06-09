using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public static class CcittFilterFactory
{
    public static async ValueTask<IStreamFilterDefinition> Encoder(PdfObject? arg) => 
        SelectEncoderType(await CcittParameters.FromPdfObject(arg).CA());

    private static IStreamFilterDefinition SelectEncoderType(CcittParameters args) => args.K switch
        {
            < 0 => new CcittType4Encoder(args),
            0 => new CcittType31dEncoder(args),
            > 0 => new CcittType3SwitchingEncoder(args)
        };

    public static async ValueTask<IStreamFilterDefinition> Decoder(PdfObject? arg) => 
        SelectDecoderType(await CcittParameters.FromPdfObject(arg).CA());

    private static IStreamFilterDefinition SelectDecoderType(CcittParameters args) => args.K switch
    {
        < 0 => new CcittType4Decoder(args, new TwoDimensionalLineCodeDictionary()),
        0 => new CcittType4Decoder(args, new Type3K0LineCodeDictionary()),
        > 0 => new CcittType4Decoder(args, new Type3SwitchingLineCodeDictionary())
    };
}