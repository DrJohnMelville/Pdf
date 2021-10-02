using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.New
{
    public interface ICipherFactory
    {
        ICipher CipherFromName(PdfName name, in ReadOnlySpan<byte> finalKey);
    }

    public class Rc4CipherFactory : ICipherFactory
    {
        public ICipher CipherFromName(PdfName name, in ReadOnlySpan<byte> finalKey) =>
            new Rc4Cipher(finalKey);
    }

    public interface IObjectCryptContext
    {
        public ICipher StringCipher();
        public ICipher StreamCipher();
        public ICipher NamedCipher(PdfName name);
        
    }

    public interface IDocumentCryptContext
    {
        IObjectCryptContext ContextForObject(int objectNumber, int generationNumber);
    }

    public class DocumentCryptContext : IDocumentCryptContext
    {
        private readonly byte[] rootKey;
        private readonly IKeySpecializer keySpecializer;
        private readonly ICipherFactory cipherFactory;

        public DocumentCryptContext(byte[] rootKey, IKeySpecializer keySpecializer, ICipherFactory cipherFactory)
        {
            this.rootKey = rootKey;
            this.keySpecializer = keySpecializer;
            this.cipherFactory = cipherFactory;
        }

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
                documentContext.cipherFactory.CipherFromName(KnownNames.StrF, KeyForObject());
            public ICipher StreamCipher() => 
                documentContext.cipherFactory.CipherFromName(KnownNames.StmF, KeyForObject());
            public ICipher NamedCipher(PdfName name) => 
                documentContext.cipherFactory.CipherFromName(name, documentContext.rootKey);
 
            private ReadOnlySpan<byte> KeyForObject() => 
                documentContext.keySpecializer.ComputeKeyForObject(
                    documentContext.rootKey, objectNumber, generationNumber);
        }
    }
}