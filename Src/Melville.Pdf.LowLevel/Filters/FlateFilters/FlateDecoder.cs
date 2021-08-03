using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters
{
    public class FlateDecoder: IDecoder
    {
        public async ValueTask<Stream> WrapStreamAsync(Stream input, PdfObject parameter)
        {
            var buffer = new byte[2];
            int totalRead = 0;
            do
            {
                var localRead = await input.ReadAsync(buffer, 0, 2 - totalRead);
                totalRead += localRead;
            } while (totalRead < 2);
            return new DeflateStream(input, CompressionMode.Decompress);
        }
    }
}