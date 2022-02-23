using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public static class CcittFilterFactory
{
    public async static ValueTask<IStreamFilterDefinition> Encoder(PdfObject? arg)
    {
        var args = await CcittParameters.FromPdfObject(arg).CA();
        return args.K >= 0 ? (IStreamFilterDefinition)new CcittType3Encoder(args) : new CcittType4Encoder(args);
    }

    public static async ValueTask<IStreamFilterDefinition> Decoder(PdfObject? arg)
    {
        var args = await CcittParameters.FromPdfObject(arg).CA();
        if (args.K >= 0)
            throw new NotSupportedException("Type 3 CCITT encoding not implemented yet.");
        return new CcittType4Decoder(args);
    }
}