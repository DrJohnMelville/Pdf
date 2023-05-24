using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers.V6SecurityHandler;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

internal static class SecurityHandlerFactory
{
    public static async ValueTask<ISecurityHandler> CreateSecurityHandlerAsync(
        PdfDictionary trailer, PdfDictionary dict)
    {
        if (await dict.GetAsync<PdfName>(KnownNames.Filter).CA() != KnownNames.Standard)
            throw new PdfSecurityException("Only standard security handler is supported.");
        
        var V = (await dict.GetAsync<PdfNumber>(KnownNames.V).CA()).IntValue;
        var R = (await dict.GetAsync<PdfNumber>(KnownNames.R).CA()).IntValue;

        var parameters = await EncryptionParameters.CreateAsync(trailer).CA();
            
        return (V,R)switch
        {
            (0 or 3, _) or (_, 5) => throw new PdfSecurityException("Undocumented Algorithms are not supported"),
            (4, _) => await CryptFilterReader.CreateAsync(RootKeyComputerV3(parameters),dict).CA(),
            (1 or 2, 2) =>  SecurityHandlerV2( parameters, dict),
            (1 or 2, 3) =>  SecurityHandlerV3(parameters, dict),
            (5, 6) => await SecurityHandlerV6Factory.CreateAsync(dict).CA(),
            (_, 4) => throw new PdfSecurityException(
                "Standard Security handler V4 requires a encryption value of 4."),
            _ => throw new PdfSecurityException("Unrecognized encryption algorithm (V)")
        };
    }

    private static ISecurityHandler SecurityHandlerV2(
        in EncryptionParameters parameters, PdfObject dict) =>
        new SecurityHandler(Rc4KeySpecializer.Instance, 
            Rc4CipherFactory.Instance, 
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
        new SecurityHandler(Rc4KeySpecializer.Instance, 
            Rc4CipherFactory.Instance, 
            RootKeyComputerV3(parameters),
            dict);

    private static RootKeyComputer RootKeyComputerV3(EncryptionParameters parameters)
    {
        return new RootKeyComputer( new GlobalEncryptionKeyComputerV3(),
            new ComputeUserPasswordV3(),
            ComputeOwnerPasswordV3.Instance,
            parameters);
    }
}