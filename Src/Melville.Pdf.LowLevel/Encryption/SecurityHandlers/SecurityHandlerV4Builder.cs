using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers
{
    public static class SecurityHandlerV4Builder
    {
        
        public static async ValueTask<SecurityHandlerV4> Create(RootKeyComputer rootKeyComputer, PdfDictionary encryptionDictionary)
        {
            var cfd = await encryptionDictionary.GetAsync<PdfDictionary>(KnownNames.CF);
            var finalDictionary = new Dictionary<PdfName, ISecurityHandler>();
            finalDictionary.Add(KnownNames.Identity, NullSecurityHandler.Instance);
            foreach (var entry in cfd)
            {
                var cryptDictionary = (PdfDictionary)await entry.Value;
                var cfm = await cryptDictionary.GetAsync<PdfName>(KnownNames.CFM);
                finalDictionary.Add(entry.Key, CreateSubSecurityHandler(rootKeyComputer, cfm, encryptionDictionary));
            }
            
            finalDictionary.Add(KnownNames.StmF, 
                finalDictionary[await encryptionDictionary.GetOrDefaultAsync(KnownNames.StmF, KnownNames.Identity)]);
            finalDictionary.Add(KnownNames.StrF, 
                finalDictionary[await encryptionDictionary.GetOrDefaultAsync(KnownNames.StrF, KnownNames.Identity)]);

            return new SecurityHandlerV4(rootKeyComputer ,finalDictionary);
        }

        private static ISecurityHandler CreateSubSecurityHandler(
            RootKeyComputer rootKeyComputer, PdfName cfm, PdfObject dict)
        {
            return cfm switch
            {
                var i when i == KnownNames.V2 =>
                    new SecurityHandler(new Rc4KeySpecializer(),
                        new Rc4CipherFactory(),
                        rootKeyComputer,
                        dict),
                var i when i == KnownNames.None => NullSecurityHandler.Instance,
                _ => throw new PdfSecurityException("Unknown Security Handler Type")
            };
        }
    }
}