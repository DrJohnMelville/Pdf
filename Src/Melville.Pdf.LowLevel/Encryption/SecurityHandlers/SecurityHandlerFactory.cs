using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

public static class SecurityHandlerFactory
{
    public static async ValueTask<ISecurityHandler> CreateSecurityHandler(
        PdfDictionary trailer, PdfDictionary dict)
    {
        if (await dict.GetAsync<PdfName>(KnownNames.Filter) != KnownNames.Standard)
            throw new PdfSecurityException("Only standard security handler is supported.");
        
        var V = (await dict.GetAsync<PdfNumber>(KnownNames.V)).IntValue;
        var R = (await dict.GetAsync<PdfNumber>(KnownNames.R)).IntValue;

        var parameters = await EncryptionParameters.Create(trailer);
            
        return (V,R)switch
        {
            (0 or 3, _) => throw new PdfSecurityException("Undocumented Algorithms are not supported"),
            (4, _) => await SecurityHandlerV4Builder.Create(RootKeyComputerV3(parameters),dict),
            (1 or 2, 2) =>  SecurityHandlerV2( parameters, dict),
            (1 or 2, 3) =>  SecurityHandlerV3(parameters, dict),
            (_, 4) => throw new PdfSecurityException(
                "Standard Security handler V4 requires a encryption value of 4."),
            _ => throw new PdfSecurityException("Unrecognized encryption algorithm (V)")
        };
    }

    private static ISecurityHandler SecurityHandlerV2(
        in EncryptionParameters parameters, PdfObject dict) =>
        new SecurityHandler(new Rc4KeySpecializer(),
            new Rc4CipherFactory(),
            RootKeyComputerV2(parameters), dict);

    private static RootKeyComputer RootKeyComputerV2(EncryptionParameters parameters)
    {
        return new RootKeyComputer(new GlobalEncryptionKeyComputerV2(),
            new ComputeUserPasswordV2(),
            new ComputeOwnerPasswordV2(),
            parameters);
    }

    private static ISecurityHandler SecurityHandlerV3(
        in EncryptionParameters parameters, PdfObject dict) =>
        new SecurityHandler(new Rc4KeySpecializer(),
            new Rc4CipherFactory(),
            RootKeyComputerV3(parameters),
            dict);

    private static RootKeyComputer RootKeyComputerV3(EncryptionParameters parameters)
    {
        return new RootKeyComputer( new GlobalEncryptionKeyComputerV3(),
            new ComputeUserPasswordV3(),
            new ComputeOwnerPasswordV3(),
            parameters);
    }
}