using System;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.CryptContexts;

public interface IDocumentCryptContext
{
    IObjectCryptContext ContextForObject(int objectNumber, int generationNumber);
    bool BlockEncryption(PdfObject item);
}
public class DocumentCryptContext : IDocumentCryptContext
{
    private readonly byte[] rootKey;
    private readonly IKeySpecializer keySpecializer;
    private readonly ICipherFactory cipherFactory;
    private readonly PdfObject? blockEncryption;

    public DocumentCryptContext(byte[] rootKey, IKeySpecializer keySpecializer, 
        ICipherFactory cipherFactory, PdfObject? blockEncryption)
    {
        this.rootKey = rootKey;
        this.keySpecializer = keySpecializer;
        this.cipherFactory = cipherFactory;
        this.blockEncryption = blockEncryption;
    }

    public bool BlockEncryption(PdfObject item) => blockEncryption == item;

    public IObjectCryptContext ContextForObject(int objectNumber, int generationNumber) =>
        new ObjectCryptContext(this, objectNumber, generationNumber);
        
    private class ObjectCryptContext : IObjectCryptContext
    {
        private readonly DocumentCryptContext documentContext;
        private readonly int objectNumber;
        private readonly int generationNumber;

        public ObjectCryptContext(DocumentCryptContext documentContext, int objectNumber, int generationNumber)
        {
            this.documentContext = documentContext;
            this.objectNumber = objectNumber;
            this.generationNumber = generationNumber;
        }

        public ICipher StringCipher() => 
            documentContext.cipherFactory.CipherFromKey(KeyForObject());
        public ICipher StreamCipher() => 
            documentContext.cipherFactory.CipherFromKey(KeyForObject());
        public ICipher NamedCipher(PdfName name) => 
            documentContext.cipherFactory.CipherFromKey(documentContext.rootKey);
 
        private ReadOnlySpan<byte> KeyForObject() => 
            documentContext.keySpecializer.ComputeKeyForObject(
                documentContext.rootKey, objectNumber, generationNumber);
    }
}