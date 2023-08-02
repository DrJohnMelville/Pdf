using System;
using System.IO;
using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

[StaticSingleton]
internal partial class NullSecurityHandler: 
    ISecurityHandler, IDocumentCryptContext, IObjectCryptContext, ICipher, ICipherOperations
{
    public bool BlockEncryption(PdfDictionary item) => false;
    public byte[]? TryComputeRootKey(string password, PasswordType type) => Array.Empty<byte>();
    public IDocumentCryptContext CreateCryptContext(byte[] rootKey) => this;
    public IObjectCryptContext ContextForObject(int objectNumber, int generationNumber) => this;
    public ICipher StringCipher() => this;
    public ICipher StreamCipher() => this;
    public ICipher NamedCipher(in PdfDirectObject name) => 
        name.Equals(KnownNames.Identity) ? this :
            throw new PdfParseException("Should not have a crypt filter in an unencrypted document.");
    public ICipherOperations Encrypt() => this;
    public ICipherOperations Decrypt() => this;
    public Span<byte> CryptSpan(Span<byte> input) => input;
    public Stream CryptStream(Stream input) => input;
}