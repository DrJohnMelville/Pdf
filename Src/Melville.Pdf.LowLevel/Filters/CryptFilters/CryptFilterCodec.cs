using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters;

internal class CryptSingleFilter: IApplySingleFilter
{
    private readonly IApplySingleFilter innerFilter;
    private readonly IStreamDataSource streamDataSource;
    private readonly IObjectCryptContext encryptor;

    public CryptSingleFilter(IStreamDataSource streamDataSource, IObjectCryptContext encryptor,
        IApplySingleFilter innerFilter)
    {
        this.innerFilter = innerFilter;
        this.streamDataSource = streamDataSource;
        this.encryptor = encryptor;
            
    }

    public async ValueTask<Stream> EncodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter)
    {
        return (filter.Equals(KnownNames.CryptTName)) ? 
            (await ComputeCipherAsync(parameter).CA()).Encrypt().CryptStream(source) : 
            await innerFilter.EncodeAsync(source, filter, parameter).CA();
    }

    private async ValueTask<ICipher> ComputeCipherAsync(PdfDirectObject parameter)
    {
        var encryptionAlg = await EncryptionAlgAsync(parameter).CA();
        var ret = encryptor.NamedCipher(encryptionAlg);
        return ret;
    }

    public async ValueTask<Stream> DecodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter)
    {
        return (filter.Equals(KnownNames.CryptTName)) ? 
            streamDataSource.WrapStreamWithDecryptor(source, await EncryptionAlgAsync(parameter).CA()) : 
            await innerFilter.DecodeAsync(source, filter, parameter).CA();
    }

    public async ValueTask<PdfDirectObject> EncryptionAlgAsync(PdfDirectObject parameter) =>
        parameter.TryGet(out PdfDictionary dict)
            ? await dict.GetOrDefaultAsync(KnownNames.NameTName, KnownNames.IdentityTName).CA()
            : KnownNames.IdentityTName;
}