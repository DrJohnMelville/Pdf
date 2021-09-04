using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption
{
    public class SecurityHandlerV4 : ISecurityHandler
    {
        private readonly Dictionary<PdfName, ISecurityHandler> handlers;
        private readonly ISecurityHandler defaultStringHandler;
        private readonly ISecurityHandler defaultStreamHandler;

        public SecurityHandlerV4(Dictionary<PdfName, ISecurityHandler> handlers,
            PdfName defStringHandlerName, PdfName defStreamHandlerName)
        {
            this.handlers = handlers;
            defaultStringHandler = handlers[defStringHandlerName];
            defaultStreamHandler = handlers[defStreamHandlerName];
        }

        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber)
        {
            #warning Needs to acutally pick the right filter.
            return defaultStreamHandler.DecryptorForObject(objectNumber, generationNumber);
        }

        public bool TyySinglePassword((string?, PasswordType) password)
        {
            return handlers.Values.All(i => i.TyySinglePassword(password));
        }
    }
    
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

            return new SecurityHandlerV4(
                finalDictionary,
                await encryptionDictionary.GetOrDefaultAsync(KnownNames.StrF, KnownNames.Identity),
                await encryptionDictionary.GetOrDefaultAsync(KnownNames.StmF, KnownNames.Identity));
        }

        private static ISecurityHandler CreateSubSecurityHandler(
            EncryptionParameters parameters, PdfName cfm, PdfNumber length)
        {
            return cfm switch
            {
                var i when i == KnownNames.V2 =>
                    new SecurityHandler(parameters,
                        new EncryptionKeyComputerV3(),
                        new ComputeUserPasswordV3(),
                        new Rc4DecryptorFactory()),
                _ => throw new PdfSecurityException("Unknown Security Handler Type")
            };
        }
    }
    
    public class NullSecurityHandler: ISecurityHandler
    {
        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber) => NullDecryptor.Instance;

        public bool TyySinglePassword((string?, PasswordType) password) => true;
    }
}