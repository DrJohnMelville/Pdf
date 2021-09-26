using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers
{
    public class LiteralStreamSource: IStreamDataSource
    {
        private MultiBufferStream source;
        public LiteralStreamSource(string data): this (data.AsExtendedAsciiBytes())
        {
        }
        public LiteralStreamSource(byte[] buffer): this(new MultiBufferStream(buffer))
        {
        }
        public LiteralStreamSource(MultiBufferStream stream)
        {
            source = stream;
        }

        public ValueTask<Stream> OpenRawStream(long streamLength, PdfStream stream)
        {
          Debug.Assert(streamLength == source.Length);
          return new ValueTask<Stream>(source.CreateReader());
        }

        public Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName cryptFilterName) =>
            encryptedStream;
    }
}