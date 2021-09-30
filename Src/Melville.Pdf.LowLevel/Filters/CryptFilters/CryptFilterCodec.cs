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
        private readonly IStreamDataSource streamDataSource;
        private readonly IObjectEncryptor encryptor;

        public CryptSingleFilter(
            IApplySingleFilter innerFilter, IStreamDataSource streamDataSource, IObjectEncryptor encryptor)
        {
            this.innerFilter = innerFilter;
            this.streamDataSource = streamDataSource;
            this.encryptor = encryptor;
            
        }

        public async ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter)
        {
            return (filter == KnownNames.Crypt) ? 
                encryptor.WrapReadingStreamWithEncryption(source, await EncryptionAlg(parameter)) : 
                await innerFilter.Encode(source, filter, parameter);
        }

        public async ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter)
        {
            return (filter == KnownNames.Crypt) ? 
                streamDataSource.WrapStreamWithDecryptor(source, await EncryptionAlg(parameter)) : 
                await innerFilter.Decode(source, filter, parameter);
        }

        public async ValueTask<PdfName> EncryptionAlg(PdfObject parameter) =>
            (await parameter.DirectValueAsync()) is PdfDictionary dict
                ? await dict.GetOrDefaultAsync(KnownNames.Name, KnownNames.Identity)
                : KnownNames.Identity;
    }
}