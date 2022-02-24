using System;
using System.Diagnostics;
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

    public static async ValueTask<IStreamFilterDefinition> Decoder(PdfObject? arg)
    {
        var args = await CcittParameters.FromPdfObject(arg).CA();
        if (args.K >= 0)
            throw new NotSupportedException("Type 3 CCITT encoding not implemented yet.");
        return new CcittType4Decoder(args);
    }
}