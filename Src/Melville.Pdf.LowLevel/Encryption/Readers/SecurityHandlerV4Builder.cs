using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public static class SecurityHandlerV4Builder
    {
        
        public static async ValueTask<SecurityHandlerV4> Create(
            EncryptionParameters encryptionParameters, PdfDictionary encryptionDictionary)
        {
            var cfd = await encryptionDictionary.GetAsync<PdfDictionary>(KnownNames.CF);
            var finalDictionary = new Dictionary<PdfName, ISecurityHandler>();
            finalDictionary.Add(KnownNames.Identity, new NullSecurityHandler());
            foreach (var entry in cfd)
            {
                var cryptDictionary = (PdfDictionary)await entry.Value;
                var cfm = await cryptDictionary.GetAsync<PdfName>(KnownNames.CFM);
                var length = await cryptDictionary.GetAsync<PdfNumber>(KnownNames.Length);
                finalDictionary.Add(entry.Key, CreateSubSecurityHandler(encryptionParameters, cfm, length));
            }
            
            finalDictionary.Add(KnownNames.StmF, 
                finalDictionary[await encryptionDictionary.GetOrDefaultAsync(KnownNames.StmF, KnownNames.Identity)]);
            finalDictionary.Add(KnownNames.StrF, 
                finalDictionary[await encryptionDictionary.GetOrDefaultAsync(KnownNames.StrF, KnownNames.Identity)]);

            return new SecurityHandlerV4(finalDictionary);
        }

        private static ISecurityHandler CreateSubSecurityHandler(
            EncryptionParameters parameters, PdfName cfm, PdfNumber length)
        {
            return cfm switch
            {
                var i when i == KnownNames.V2 =>
                    new SecurityHandler(parameters,
                        new GlobalEncryptionKeyComputerV3(),
                        new ComputeUserPasswordV3(),
                        new ComputeOwnerPasswordV3(),
                        new Rc4DecryptorFactory()),
                var i when i == KnownNames.None => new NullSecurityHandler(),
                _ => throw new PdfSecurityException("Unknown Security Handler Type")
            };
        }
    }
}