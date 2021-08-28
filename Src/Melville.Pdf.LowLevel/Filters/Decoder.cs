using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters
{
    public static class Decoder
    {
        public static async ValueTask<Stream> DecodeStream(
            Stream source, IReadOnlyList<PdfObject> filters, 
            IReadOnlyList<PdfObject> parameters, int desiredFormat)
        {
            desiredFormat = Math.Min(desiredFormat, filters.Count);
            for (var i = 0; i < desiredFormat; i++)
            {
                var parameter = ComputeParameter(parameters, i);
                source = await DecodeSingleStream(source, filters[i], parameter);
                source = await CodecFactory.PredictionCodec.DecodeOnReadStream(source, parameter);
            }
            return source;
        }

        private static PdfObject ComputeParameter(IReadOnlyList<PdfObject> parameters, int i) => 
            i < parameters.Count ? parameters[i] : PdfTokenValues.Null;

        private static ValueTask<Stream> DecodeSingleStream(
            Stream source, PdfObject filter, PdfObject parameter) => 
            CodecFactory.CodecFor((PdfName) filter).DecodeOnReadStream(source, parameter);
    }
}