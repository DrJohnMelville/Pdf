using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters
{
    public static class Encode
    {
        public static ValueTask<Stream> Compress(in StreamDataSource data, PdfObject algorithm, PdfObject? parameters)
        {
            var algorithms = algorithm.AsList();
            return DoCompress(data.Stream, algorithms, parameters.AsList(), algorithms.Count - 1);
        }
        private static async ValueTask<Stream> DoCompress(Stream data, IReadOnlyList<PdfObject> algorithms, IReadOnlyList<PdfObject> parameters, int which)
        {
            for (var i = which; i >= 0; i--)
            {
                var singleCodecParems = TryGetParam(parameters, i);
                data = await CodecFactory.PredictionCodec.EncodeOnReadStream(data, singleCodecParems);
                data = await CodecFactory.CodecFor((PdfName)algorithms[i])
                    .EncodeOnReadStream(data, singleCodecParems);
            }
            return data;
        }
        public static ValueTask<Stream> CompressOnWrite(in StreamDataSource data, PdfObject algorithm, PdfObject? parameters)
        {
            var algorithms = algorithm.AsList();
            return DoCompressOnWrite(data.Stream, algorithms, parameters.AsList(), algorithms.Count);
        }
        private static async ValueTask<Stream> DoCompressOnWrite(
            Stream data, IReadOnlyList<PdfObject> algorithms, IReadOnlyList<PdfObject> parameters, int which)
        {
            for (var i = 0; i < which; i++)
            {
                var singleCodecParems = TryGetParam(parameters, i);
                data = await CodecFactory.CodecFor((PdfName)algorithms[i])
                    .EncodeOnWriteStream(data, singleCodecParems);
                data = await CodecFactory.PredictionCodec.EncodeOnWriteStream(data, singleCodecParems);
            }
            return data;
        }

        private static PdfObject TryGetParam(IReadOnlyList<PdfObject> parameters, int i) => 
            i < parameters.Count ? parameters[i] : PdfTokenValues.Null;
    }
}