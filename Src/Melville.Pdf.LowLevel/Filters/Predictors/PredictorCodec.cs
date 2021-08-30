using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.Predictors
{
    public class PredictorCodec : ICodecDefinition
    {
        private const int FirstPngPredictor = 10;
        private const int LastPngPredictor = 15;
        private const int TiffPredictor2 = 2;
        private const int NoPredictor = 1;

        public async ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters)
        {
            var filter = await PredictionFilterAsync(parameters, true);
            return filter == null ? data : ReadingFilterStream.Wrap(data, filter);
        }

        public async ValueTask<Stream> EncodeOnWriteStream(Stream data, PdfObject? parameters)
        {
            var filter = await PredictionFilterAsync(parameters, true);
            return filter == null ? data : new WritingFilterStream(data, filter);
        }

        public async ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
        {
            var filter = await PredictionFilterAsync(parameters, false);
            return filter == null ? input : ReadingFilterStream.Wrap(input, filter);
        }

        private async ValueTask<IStreamFilterDefinition?> PredictionFilterAsync(
            PdfObject? parameters, bool encoding) =>
            parameters is not PdfDictionary dict
                ? null
                : PredictionFilter(
                    await dict.GetOrDefaultAsync(KnownNames.Predictor, 1),
                    (int)await dict.GetOrDefaultAsync(KnownNames.Colors, 1),
                    (int)await dict.GetOrDefaultAsync(KnownNames.BitsPerComponent, 8),
                    (int)await dict.GetOrDefaultAsync(KnownNames.Columns, 1), 
                    encoding);

        private IStreamFilterDefinition? PredictionFilter(
            long predictor, int colors, int bitsPerCompnent, int columns, bool encoding) => predictor switch
            {
                NoPredictor => null,
                TiffPredictor2 => encoding?
                    new TiffPredictor2Encoder(colors, bitsPerCompnent, columns):
                    new TiffPredictor2Decoder(colors, bitsPerCompnent, columns),
                >= FirstPngPredictor and <= LastPngPredictor =>
                    CreatePngFilter((byte)(predictor - FirstPngPredictor), encoding,
                       ScanLineLengthComputer.ComputeGroupsPerRow(colors, bitsPerCompnent, columns, 8)),
                _ => throw new InvalidOperationException("Unknown Predictor type")
            };

        private IStreamFilterDefinition? CreatePngFilter(
            byte predictorIndex, bool encoding, long strideInBytes) =>
            encoding
                ? new PngPredictingEncoder(predictorIndex, (int)strideInBytes)
                : new PngPredictingDecoder((int)strideInBytes);
    }
}