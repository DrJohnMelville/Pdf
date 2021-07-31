using System.IO;
using System.IO.Pipelines;

namespace Melville.Pdf.LowLevel.Filters
{
    public static class StreamToPipeReader
    {
        private static readonly StreamPipeReaderOptions options = new();
        public static PipeReader AsPipeReader(this Stream s) =>
            PipeReader.Create(s, options);
    }
}