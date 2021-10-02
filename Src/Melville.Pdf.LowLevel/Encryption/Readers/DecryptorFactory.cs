using System;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Encryption.Writers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Parsing.Decryptors;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public interface IEncryptorAndDecryptorFactory
    {
        IDecryptor CreateDecryptor(byte[] baseKey, int objectNumber, int generationNumber);
        IObjectEncryptor CreateEncryptor(byte[] baseKey, int objectNumber, int generationNumber);
    }
    
    public abstract class EncryptorAndDecryptorFactory: IEncryptorAndDecryptorFactory
    {
        public IDecryptor CreateDecryptor(byte[] baseKey, int objectNumber, int generationNumber) => 
            CreateDecryptor(ObjectEncryptionKey(baseKey, objectNumber, generationNumber));

        public IObjectEncryptor CreateEncryptor(byte[] baseKey, int objectNumber, int generationNumber) =>
            CreateEncryptor(ObjectEncryptionKey(baseKey, objectNumber, generationNumber));

        private static ReadOnlySpan<byte> ObjectEncryptionKey(byte[] baseKey, int objectNumber, int generationNumber)
        {
#warning -- this line is a hack that needs to go away.  eventually I ought to take
            return objectNumber < 0 ? baseKey : new Rc4KeySpecializer().ComputeKeyForObject(baseKey, objectNumber, generationNumber);
        }

        protected abstract IDecryptor CreateDecryptor(in ReadOnlySpan<byte> encryptionKey);
        protected abstract IObjectEncryptor CreateEncryptor(in ReadOnlySpan<byte> encryptionKey);
    }

    #warning get rid of this inheritence as well
    public class Rc4DecryptorFactory : EncryptorAndDecryptorFactory
    {
        protected override IDecryptor CreateDecryptor(in ReadOnlySpan<byte> encryptionKey) => 
            new Rc4Decryptor(encryptionKey);

        protected override IObjectEncryptor CreateEncryptor(in ReadOnlySpan<byte> encryptionKey) =>
            new Rc4Encryptor(encryptionKey);
    }
}