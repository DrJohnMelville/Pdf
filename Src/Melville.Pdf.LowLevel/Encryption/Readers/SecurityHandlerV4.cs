using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public class SecurityHandlerV4 : ISecurityHandler
    {
        private readonly Dictionary<PdfName, ISecurityHandler> handlers;

        public SecurityHandlerV4(Dictionary<PdfName, ISecurityHandler> handlers)
        {
            this.handlers = handlers;
        }

        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfName? cryptFilterForName)
        {
            return PickHandler(cryptFilterForName)
                .DecryptorForObject(objectNumber, generationNumber, cryptFilterForName);
        }

        private ISecurityHandler PickHandler(PdfName cryptFilter) => handlers[cryptFilter];
        
        public bool TrySinglePassword((string?, PasswordType) password) => 
            handlers.Values.All(i => i.TrySinglePassword(password));

        public IObjectEncryptor EncryptorForObject(PdfIndirectObject parent, PdfName cryptFilterName)
        {
            return PickHandler(cryptFilterName).EncryptorForObject(parent, cryptFilterName);
        }
    }

    public class NullSecurityHandler: ISecurityHandler
    {
        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfName cryptFilterName) =>
            NullDecryptor.Instance;

        public bool TrySinglePassword((string?, PasswordType) password) => true;
        
        public IObjectEncryptor EncryptorForObject(PdfIndirectObject parent, PdfName cryptFilterName)
        {
            return NullObjectEncryptor.Instance;
        }
    }
}