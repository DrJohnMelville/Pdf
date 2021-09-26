using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters
{
    public class CryptFilterCodec: ICodecDefinition
    {
        public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters) => new(data);
        public ValueTask<Stream> EncodeOnWriteStream(Stream data, PdfObject? parameters) => new(data);
        public ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters) => new(input);
    }
}