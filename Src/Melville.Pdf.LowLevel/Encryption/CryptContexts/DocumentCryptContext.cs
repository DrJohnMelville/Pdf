using System;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Encryption.CryptContexts;

internal interface IDocumentCryptContext
{
    /// <summary>
    /// Creates an Object encryption context for an object with the given object and generation number.
    /// In the pre v6 encryption schemes, every object is encrypted with a unique key.
    /// </summary>
    /// <param name="objectNumber">The object number of the item to be encrypted</param>
    /// <param name="generationNumber">The generation number of the item to be encrypted.'</param>
    /// <returns></returns>
    IObjectCryptContext ContextForObject(int objectNumber, int generationNumber);
    /// <summary>
    /// Determine whether or not the given object should be encrypted.  Some objects, like the document root and encryption dictionary
    /// cannot be enrypted because a reader must parse them to get the encryption parameters to create a decryptor
    /// </summary>
    /// <param name="item">A PDF object</param>
    /// <returns></returns>
    bool BlockEncryption(PdfValueDictionary item);
}
internal class DocumentCryptContext : IDocumentCryptContext
{
    private readonly byte[] rootKey;
    private readonly IKeySpecializer keySpecializer;
    private readonly ICipherFactory cipherFactory;
    private readonly PdfValueDictionary? blockEncryption;

    public DocumentCryptContext(byte[] rootKey, IKeySpecializer keySpecializer, 
        ICipherFactory cipherFactory, PdfValueDictionary? blockEncryption)
    {
        this.rootKey = rootKey;
        this.keySpecializer = keySpecializer;
        this.cipherFactory = cipherFactory;
        this.blockEncryption = blockEncryption;
    }

    public bool BlockEncryption(PdfValueDictionary item) => blockEncryption == item;

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
        public ICipher NamedCipher(in PdfDirectValue name) => 
            documentContext.cipherFactory.CipherFromKey(documentContext.rootKey);
 
        private ReadOnlySpan<byte> KeyForObject() => 
            documentContext.keySpecializer.ComputeKeyForObject(
                documentContext.rootKey, objectNumber, generationNumber);
    }
}