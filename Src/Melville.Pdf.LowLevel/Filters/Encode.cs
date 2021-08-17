using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;

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
                data = await CodecFactory.CodecFor((PdfName)algorithms[i])
                    .EncodeOnReadStream(data, TryGetParam(parameters, i));
            }
            return data;
        }

        private static PdfObject TryGetParam(IReadOnlyList<PdfObject> parameters, int i) => 
            i < parameters.Count ? parameters[i] : PdfTokenValues.Null;
    }
}