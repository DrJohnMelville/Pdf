using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters
{
    public class FlateEncoder:IEncoder
    {
        private static byte[] prefix = {0x78, 0x49};
        public byte[] Encode(byte[] data, PdfObject? parameters)
        {
            var ret = new MemoryStream();
            ret.Write(prefix, 0, 2);
            var deflate = new DeflateStream(ret, CompressionLevel.Optimal);
            deflate.Write(data);
            deflate.Flush();
            return ret.ToArray();
        }
    }
    
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