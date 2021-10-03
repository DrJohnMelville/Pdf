using System;
using System.IO;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public class NullSecurityHandler: 
        ISecurityHandler, IDocumentCryptContext, IObjectCryptContext, ICipher, ICipherOperations
    {
        public static readonly NullSecurityHandler Instance = new();
        private NullSecurityHandler()
        {
        }

        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfName cryptFilterName) =>
            NullDecryptor.Instance;

        public bool BlockEncryption(PdfObject item) => false;
        public bool TrySinglePassword((string?, PasswordType) password) => true;
        public byte[]? TryComputeRootKey(string password, PasswordType type) => Array.Empty<byte>();
        public IDocumentCryptContext CreateCryptContext(byte[] rootKey) => this;
        public IObjectEncryptor EncryptorForObject(int objNum, int generationNumber) => NullObjectEncryptor.Instance;
        public IObjectCryptContext ContextForObject(int objectNumber, int generationNumber) => this;
        public ICipher StringCipher() => this;
        public ICipher StreamCipher() => this;
        public ICipher NamedCipher(PdfName name) =>
            (name == KnownNames.Identity) ? this :
            throw new PdfParseException("Should not have a crypt filter in an unencrypted document.");
        public ICipherOperations Encrypt() => this;
        public ICipherOperations Decrypt() => this;
        public byte[] CryptSpan(byte[] input) => input;
        public Stream CryptStream(Stream input) => input;
    }
}