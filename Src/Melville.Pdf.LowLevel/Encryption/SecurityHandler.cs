using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.Cryptography;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface ISecurityHandler
    {
        bool TyyUserPassword(in Span<byte> password);
    }

    public static class SecurityHandlerOperations
    {
        public static bool TryUserPassword(this ISecurityHandler handler, string password) =>
          handler.TyyUserPassword(password.AsExtendedAsciiBytes());
    }

    public static class SecurityHandlerFactory
    {
        public static async ValueTask<ISecurityHandler> CreateSecurityHandler(PdfDictionary trailer)
        {
            if (await trailer.GetOrNullAsync(KnownNames.Encrypt) is not PdfDictionary dict)
                throw new PdfSecurityException("Not encrypted");
                
            if (await dict.GetAsync<PdfName>(KnownNames.Filter) != KnownNames.Standard)
                throw new PdfSecurityException("Only standard security handler is supported.");
        
            var V = (await dict.GetAsync<PdfNumber>(KnownNames.V)).IntValue;
            var R = (await dict.GetAsync<PdfNumber>(KnownNames.R)).IntValue;
            
            return (V,R)switch
            {
                (0 or 3, _) => throw new PdfSecurityException("Undocumented Algorithms are not supported"),
                (4, _) => throw new PdfSecurityException("Default CryptFilters are not supported."),
                (1 or 2, 2) =>  SecurityHandlerV2( await EncryptionParameters.Create(trailer)),
                (1 or 2, 3) =>  SecurityHandlerV3(await EncryptionParameters.Create(trailer)),
                (_, 4) => throw new PdfSecurityException(
                    "Standard Security handler V4 requires a encryption value of 4  and is unsupported."),
                _ => throw new PdfSecurityException("Unrecognized encryption algorithm (V)")
            };
        }

        private static ISecurityHandler SecurityHandlerV2(in EncryptionParameters parameters) =>
            new SecurityHandler(parameters,
                new EncryptionKeyComputerV2(), 
                new ComputeUserPasswordV2());

        private static ISecurityHandler SecurityHandlerV3(in EncryptionParameters parameters) =>
            new SecurityHandler(parameters,
                new EncryptionKeyComputerV3(),
                new ComputeUserPasswordV3());
    }

    public class SecurityHandler : ISecurityHandler
    {
        private readonly EncryptionParameters Parameters;
        private readonly IEncryptionKeyComputer keyComputer;
        private readonly IComputeUserPassword userHashComputer;
        private byte[] encryptionKey;
        
        public SecurityHandler(
            EncryptionParameters parameters, 
            IEncryptionKeyComputer keyComputer, 
            IComputeUserPassword userHashComputer)
        {
            Parameters = parameters;
            this.keyComputer = keyComputer;
            this.userHashComputer = userHashComputer;
        }

        public bool TyyUserPassword(in Span<byte> password)
        {
            var key = keyComputer.ComputeKey(password, Parameters);
            var userHash = userHashComputer.ComputeHash(key, Parameters);
            if ((userHashComputer.CompareHashes(userHash, Parameters.UserPasswordHash)))
            {
                encryptionKey = key;
                return true;
            }
            return false;
        }
    }
}