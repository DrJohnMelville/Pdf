using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.Readers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Encryption.Writers
{
    public static class SecurityHandlerEncryptorFactory
    {
        public static async ValueTask<IDocumentEncryptor> CreateDocumentEncryptor(
            PdfDictionary trailer, string? userPassword)
        {
            if (await trailer.GetOrNullAsync(KnownNames.Encrypt) is not PdfDictionary dict)
                return NullDocumentEncryptor.Instance;
            var securityHandler = await SecurityHandlerFactory.CreateSecurityHandler(trailer, dict);
            if (!securityHandler.TrySinglePassword((userPassword, PasswordType.User)))
                throw new ArgumentException("Incorrect user key for encryption");
            return new SecurityHandlerDocumentEncryptor(securityHandler, 
                trailer.RawItems[KnownNames.Encrypt]);
        }
    }
    

    public class SecurityHandlerDocumentEncryptor : IDocumentEncryptor
    {
        private ISecurityHandler handler;
        private readonly PdfIndirectObject? encryptDictionaryReference;

        public SecurityHandlerDocumentEncryptor(ISecurityHandler handler, 
            PdfObject encryptDictionaryReference)
        {
            this.handler = handler;
            this.encryptDictionaryReference = GetEncryptDictReferenceObject(encryptDictionaryReference) ;
        }

        private PdfIndirectObject? GetEncryptDictReferenceObject(PdfObject obj) =>
            obj is PdfIndirectReference pir ? pir.Target : null;

        public IObjectEncryptor CreateEncryptor(PdfIndirectObject parentObject, PdfName cryptFilterName) =>
            IsEncryptDictionary(parentObject) ?
                NullObjectEncryptor.Instance : 
                handler.EncryptorForObject(parentObject, cryptFilterName);

        private bool IsEncryptDictionary(PdfIndirectObject parentObject) => 
            encryptDictionaryReference == parentObject;
    }
    
    
}