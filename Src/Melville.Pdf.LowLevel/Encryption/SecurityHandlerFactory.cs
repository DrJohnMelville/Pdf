using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption
{
    public static class SecurityHandlerFactory
    {
        public static async ValueTask<IWrapReaderForDecryption> CreateDecryptorFactory(
            PdfDictionary trailer, IPasswordSource passwordSource)
        {
            if (await trailer.GetOrNullAsync(KnownNames.Encrypt) is not PdfDictionary dict)
                return NullWrapReaderForDecryption.Instance;
            var securityHandler = await CreateSecurityHandler(trailer, dict);
            await securityHandler.TryInteactiveLogin(passwordSource);
            return new SecurityHandlerWrapReaderForDecryption(securityHandler);
        }      
        
        public static async ValueTask<ISecurityHandler> CreateSecurityHandler(PdfDictionary trailer, PdfDictionary dict)
        {
            if (await dict.GetAsync<PdfName>(KnownNames.Filter) != KnownNames.Standard)
                throw new PdfSecurityException("Only standard security handler is supported.");
        
            var V = (await dict.GetAsync<PdfNumber>(KnownNames.V)).IntValue;
            var R = (await dict.GetAsync<PdfNumber>(KnownNames.R)).IntValue;
            
            return (V,R)switch
            {
                (0 or 3, _) => throw new PdfSecurityException("Undocumented Algorithms are not supported"),
                (4, _) => await SecurityHandlerV4Builder.Create(await EncryptionParameters.Create(trailer), dict),
                (1 or 2, 2) =>  SecurityHandlerV2( await EncryptionParameters.Create(trailer)),
                (1 or 2, 3) =>  SecurityHandlerV3(await EncryptionParameters.Create(trailer)),
                (_, 4) => throw new PdfSecurityException(
                    "Standard Security handler V4 requires a encryption value of 4."),
                _ => throw new PdfSecurityException("Unrecognized encryption algorithm (V)")
            };
        }

        private static ISecurityHandler SecurityHandlerV2(in EncryptionParameters parameters) =>
            new SecurityHandler(parameters,
                new EncryptionKeyComputerV2(), 
                new ComputeUserPasswordV2(),
                new ComputeOwnerPasswordV2(),
                new Rc4DecryptorFactory());

        private static ISecurityHandler SecurityHandlerV3(in EncryptionParameters parameters) =>
            new SecurityHandler(parameters,
                new EncryptionKeyComputerV3(),
                new ComputeUserPasswordV3(),
                new ComputeOwnerPasswordV3(),
                new Rc4DecryptorFactory());

    }
}