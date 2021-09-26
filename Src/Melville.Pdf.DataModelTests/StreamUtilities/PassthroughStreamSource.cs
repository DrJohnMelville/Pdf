using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.StreamUtilities
{
    public class PassthroughStreamSource: IStreamDataSource
    {
        private readonly Stream data;

        public PassthroughStreamSource(Stream data)
        {
            this.data = data;
        }

        public ValueTask<Stream> OpenRawStream(long streamLength) => new(data);
        public Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName? cryptFilterName) => encryptedStream;

    }
}