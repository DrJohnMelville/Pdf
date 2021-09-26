using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public static class SecurityHandlerDecryptorFactory
    {
        public static async ValueTask<IWrapReaderForDecryption> CreateDecryptorFactory(
            PdfDictionary trailer, IPasswordSource passwordSource)
        {
            if (await trailer.GetOrNullAsync(KnownNames.Encrypt) is not PdfDictionary dict)
                return NullWrapReaderForDecryption.Instance;
            var securityHandler = await SecurityHandlerFactory.CreateSecurityHandler(trailer, dict);
            await securityHandler.TryInteactiveLogin(passwordSource);
            return new SecurityHandlerWrapReaderForDecryption(securityHandler);
        }
    }
    public class SecurityHandlerWrapReaderForDecryption : IWrapReaderForDecryption
    {
        private readonly ISecurityHandler securityHandler;

        public SecurityHandlerWrapReaderForDecryption(ISecurityHandler securityHandler)
        {
            this.securityHandler = securityHandler;
       }

        public IParsingReader Wrap(IParsingReader reader, int objectNumber, int generationNumber) => 
            new DecryptingParsingReader(reader, securityHandler, objectNumber, generationNumber);
    }
    
    public partial class DecryptingParsingReader : IParsingReader, IDecryptor
    {
        [DelegateTo()]
        private readonly IParsingReader inner;
        private ISecurityHandler handler;
        private int objectNumber;
        private int generationNumber;

        public DecryptingParsingReader(
            IParsingReader inner, ISecurityHandler handler, int objectNumber, int generationNumber)
        {
            this.inner = inner;
            this.handler = handler;
            this.objectNumber = objectNumber;
            this.generationNumber = generationNumber;
        }

        public IDecryptor Decryptor() => this;
        
        //Most indirect objects do not require decryption, creationg a decryptor is both and object
        //to GC and computation to find a specific key.  We put this off as long as we can by
        //implementing IDecryptor ourself, and then genenrating the real IDecryptor just in time
        //for any operations.  I could cache the decryptor, but do not do that right now.
        private IDecryptor CreateDecryptor(PdfName cryptFilterName) => 
            handler.DecryptorForObject(objectNumber, generationNumber, cryptFilterName);

        public void DecryptStringInPlace(PdfString input) =>
            CreateDecryptor(KnownNames.StrF).DecryptStringInPlace(input);

        public Stream WrapRawStream(Stream input, PdfName cryptFilterName)
        {
            return CreateDecryptor(cryptFilterName).WrapRawStream(input, cryptFilterName);
        }
    }

}