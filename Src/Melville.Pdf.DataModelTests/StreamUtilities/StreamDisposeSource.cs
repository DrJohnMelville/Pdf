using System.IO;
using System.Net;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.StreamUtilities
{
    public partial class StreamDisposeSource : Stream, IStreamDataSource
    {
        public bool IsDisposed { get; private set; }
        [DelegateTo()]
        private readonly Stream source;

        public StreamFormat SourceFormat { get; }

        public StreamDisposeSource(Stream source, StreamFormat sourceFormat)
        {
            this.source = source;
            SourceFormat = sourceFormat;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            source.Dispose();
        }

        public override ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return source.DisposeAsync();
        }

        public override void Close() => Dispose(true);

        public ValueTask<Stream> OpenRawStream(long streamLength) => new(this);

        public Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName cryptFilterName) =>
            encryptedStream;
        public Stream WrapStreamWithDecryptor(Stream encryptedStream) =>
            encryptedStream;
    }
}