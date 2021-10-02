using System;
using System.Security.Cryptography;
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

        private static Span<byte> ObjectEncryptionKey(byte[] baseKey, int objectNumber, int generationNumber)
        {
            #warning -- this line is a hack that needs to go away.  eventually I ought to take
            if (objectNumber < 0) return baseKey;
            var span = ComputeHash(baseKey, objectNumber, generationNumber).AsSpan();
            var desiredLength = EncryptionKeyLength(baseKey.Length);
            return span[..desiredLength];
        }

        private static byte[] ComputeHash(byte[] baseKey, int objectNumber, int generationNumber)
        {
            var md5 = MD5.Create();
            md5.AddData(baseKey);
            AddObjectData(objectNumber, generationNumber, md5);
            md5.FinalizeHash();
            return md5.Hash ?? throw new InvalidProgramException("Should have a hash here.");
        }

        private static void AddObjectData(int objectNumber, int generationNumber, MD5 md5)
        {
            md5.AddData(new[]
            {
                (byte)objectNumber,
                (byte)(objectNumber >> 8),
                (byte)(objectNumber >> 16),
                (byte)generationNumber,
                (byte)(generationNumber >> 8),
            });
        }

        private static int EncryptionKeyLength(int baseKeyLength) => Math.Min(baseKeyLength + 5, 16);

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