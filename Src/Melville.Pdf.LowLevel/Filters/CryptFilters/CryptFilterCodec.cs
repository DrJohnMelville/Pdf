using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters
{
    public class CryptSingleFilter: IApplySingleFilter
    {
        private readonly IApplySingleFilter innerFilter;
        public CryptSingleFilter(IApplySingleFilter innerFilter)
        {
            this.innerFilter = innerFilter;
        }

        public ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter)
        {
            return (filter == KnownNames.Crypt) ? new(source) : innerFilter.Encode(source, filter, parameter);
        }

        public ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter)
        {
            return (filter == KnownNames.Crypt) ? new(source) : innerFilter.Decode(source, filter, parameter);
        }
    }

    public class CryptFilterCodec: ICodecDefinition
    {
        public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters) => new(data);
        public ValueTask<Stream> EncodeOnWriteStream(Stream data, PdfObject? parameters) => new(data);
        public ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters) => new(input);
    }
}