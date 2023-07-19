using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters.Predictors;

[StaticSingleton()]
internal partial class PredictorCodec : ICodecDefinition
{
    private const int FirstPngPredictor = 10;
    private const int LastPngPredictor = 15;
    private const int TiffPredictor2 = 2;
    private const int NoPredictor = 1;

    public async ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectValue parameters)
    {
        var filter = await PredictionFilterAsync(parameters, true).CA();
        return filter == null ? data : ReadingFilterStream.Wrap(data, filter);
    }
        
    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectValue parameters)
    {
        var filter = await PredictionFilterAsync(parameters, false).CA();
        return filter == null ? input : ReadingFilterStream.Wrap(input, filter);
    }

    private async ValueTask<IStreamFilterDefinition?> PredictionFilterAsync(
        PdfDirectValue parameters, bool encoding) =>
        !parameters.TryGet(out PdfValueDictionary dict)
            ? null
            : PredictionFilter(
                await dict.GetOrDefaultAsync(KnownNames.PredictorTName, 1).CA(),
                (int)await dict.GetOrDefaultAsync(KnownNames.ColorsTName, 1).CA(),
                (int)await dict.GetOrDefaultAsync(KnownNames.BitsPerComponentTName, 8).CA(),
                (int)await dict.GetOrDefaultAsync(KnownNames.ColumnsTName, 1).CA(), 
                encoding);

    private IStreamFilterDefinition? PredictionFilter(
        long predictor, int colors, int bitsPerColor, int columns, bool encoding) => predictor switch
    {
        NoPredictor => null,
        TiffPredictor2 => CreateTiffPredictor(colors, bitsPerColor, columns, encoding),
        >= FirstPngPredictor and <= LastPngPredictor =>
            CreatePngPredictor(colors, bitsPerColor, columns, encoding, PredictorIndex(predictor)),
        _ => throw new PdfParseException("Unknown Predictor type")
    };

    private static byte PredictorIndex(long predictor) => (byte)(predictor - FirstPngPredictor);

    private static IStreamFilterDefinition? CreateTiffPredictor(
        int colors, int bitsPerColor, int columns, bool encoding) => encoding
        ? new TiffPredictor2Encoder(colors, bitsPerColor, columns)
        : new TiffPredictor2Decoder(colors, bitsPerColor, columns);

    private static IStreamFilterDefinition? CreatePngPredictor(
        int colors, int bitsPerColor, int columns, bool encoding, byte predictorIndex) => encoding
        ? new PngPredictingEncoder(colors, bitsPerColor, columns, predictorIndex)
        : new PngPredictingDecoder(colors, bitsPerColor, columns);
}