using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Encryption.Readers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.Writers
{
    public static class DocumentCryptContextFactory
    {
        public static async ValueTask<IDocumentCryptContext> CreateCryptContext(
            PdfDictionary trailer, string? userPassword)
        {
            if (await trailer.GetOrNullAsync(KnownNames.Encrypt) is not PdfDictionary dict)
                return NullSecurityHandler.Instance;
            var securityHandler = await SecurityHandlerFactory.CreateSecurityHandler(trailer, dict);
            if (userPassword == null)
                throw new ArgumentException("Must specify a password to write encrypted documents");
            var key = securityHandler.TryComputeRootKey(userPassword, PasswordType.User);
            return key is null ?
                throw new ArgumentException("Incorrect user key for encryption"):
            securityHandler.CreateCryptContext(key);
        }
    }
}