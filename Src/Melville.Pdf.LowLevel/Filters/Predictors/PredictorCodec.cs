using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

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
            PdfObject? parameters, bool encoding)
        {
            if (parameters is not PdfDictionary dict) return null;
            var predictor = await dict.GetOrDefaultAsync(KnownNames.Predictor, 1);
            return predictor switch
            {
                NoPredictor => null,
                TiffPredictor2 => throw new NotImplementedException("Tiff Predictor 2 is not implemented"),
                >= FirstPngPredictor and <= LastPngPredictor =>
                    CreatePngFilter((byte)(predictor - FirstPngPredictor), encoding,
                        await dict.GetOrDefaultAsync(KnownNames.Colors, 1) *
                        await dict.GetOrDefaultAsync(KnownNames.BitsPerComponent, 8) *
                        await dict.GetOrDefaultAsync(KnownNames.Columns, 1)),
                _ => throw new InvalidOperationException("Unknown Predictor type")
            };
        }

        private IStreamFilterDefinition? CreatePngFilter(
            byte predictorIndex, bool encoding, long strideInBytes) =>
            encoding
                ? new PngPredictingEncoder(predictorIndex, (int)strideInBytes)
                : new PngPredictingDecoder((int)strideInBytes);
    }
}