using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public class SecurityHandlerV4 : ISecurityHandler
    {
        private readonly Dictionary<PdfName, ISecurityHandler> handlers;

        public SecurityHandlerV4(Dictionary<PdfName, ISecurityHandler> handlers)
        {
            this.handlers = handlers;
        }

        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfName cryptFilterForName) =>
            PickHandler(cryptFilterForName)
                .DecryptorForObject(objectNumber, generationNumber, cryptFilterForName);

        private ISecurityHandler PickHandler(PdfName cryptFilter) => handlers[cryptFilter];
        
        public bool TrySinglePassword((string?, PasswordType) password) => 
            handlers.Values.All(i => i.TrySinglePassword(password));

        public IObjectEncryptor EncryptorForObject(int objNum, int generationNumber) => 
            new InnerEncryptor(this, objNum, generationNumber);

        private class InnerEncryptor: IObjectEncryptor
        {
            private readonly SecurityHandlerV4 securityHandler;
            private readonly int objNum;
            private readonly int generationNumber;

            public InnerEncryptor(SecurityHandlerV4 securityHandler, int objNum, int generationNumber)
            {
                this.securityHandler = securityHandler;
                this.objNum = objNum;
                this.generationNumber = generationNumber;
            }

            public ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input) =>
                securityHandler.PickHandler(KnownNames.StrF)
                    .EncryptorForObject(objNum, generationNumber)
                    .Encrypt(input);

            public Stream WrapReadingStreamWithEncryption(Stream stream)
            {
               return securityHandler.PickHandler(KnownNames.StmF)
                    .EncryptorForObject(objNum, generationNumber)
                    .WrapReadingStreamWithEncryption(stream);
            }
            public Stream WrapReadingStreamWithEncryption(Stream stream, PdfName encryptionAlg)
            {
                return securityHandler.PickHandler(encryptionAlg)
                    .EncryptorForObject(-1, -1)
                    .WrapReadingStreamWithEncryption(stream, encryptionAlg);
            }
        }

    }

    public class NullSecurityHandler: ISecurityHandler
    {
        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfName cryptFilterName) =>
            NullDecryptor.Instance;

        public bool TrySinglePassword((string?, PasswordType) password) => true;
        
        public IObjectEncryptor EncryptorForObject(int objNum, int generationNumber)
        {
            return NullObjectEncryptor.Instance;
        }
    }
}