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

        public ValueTask<Stream> OpenRawStream(long streamLength, PdfStream stream) => new(data);
    }
}