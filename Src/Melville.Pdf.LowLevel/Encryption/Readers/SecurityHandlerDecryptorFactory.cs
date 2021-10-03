using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public static class SecurityHandlerDecryptorFactory
    {
        public static async ValueTask<IDocumentCryptContext> CreateDecryptorFactory(
            PdfDictionary trailer, IPasswordSource passwordSource)
        {
            if (await trailer.GetOrNullAsync(KnownNames.Encrypt) is not PdfDictionary dict)
                return NullSecurityHandler.Instance;
            var securityHandler = await SecurityHandlerFactory.CreateSecurityHandler(trailer, dict);
            return await securityHandler.InteractiveGetCryptContext(passwordSource);
        }
    }

    public partial class EncryptingParsingReader : IParsingReader
    {
        [DelegateTo]
        private readonly IParsingReader inner;
        private readonly IObjectCryptContext cryptContext;

        public EncryptingParsingReader(IParsingReader inner, IObjectCryptContext cryptContext)
        {
            this.inner = inner;
            this.cryptContext = cryptContext;
        }

        public IObjectCryptContext ObjectCryptContext() => cryptContext;
    }
}