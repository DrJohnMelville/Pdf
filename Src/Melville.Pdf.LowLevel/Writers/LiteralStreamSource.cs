using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers
{
    public class LiteralStreamSource: IStreamDataSource
    {
        private byte[] buffer;
        public LiteralStreamSource(string data): this (data.AsExtendedAsciiBytes())
        {
        }
        public LiteralStreamSource(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public ValueTask<Stream> OpenRawStream(long streamLength)
        {
          Debug.Assert(streamLength == buffer.Length);
          return new ValueTask<Stream>(new MemoryStream(buffer));
        }
    }
}