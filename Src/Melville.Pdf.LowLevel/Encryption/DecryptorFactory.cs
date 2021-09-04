using System;
using System.IO;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Encryption.Cryptography;
using Melville.Pdf.LowLevel.Parsing.Decryptors;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface IDecryptorFactory
    {
        IDecryptor CreateDecryptor(byte[] baseKey, int objectNumber, int generationNumber);
    }

    public abstract class DecryptorFactory: IDecryptorFactory
    {
        public IDecryptor CreateDecryptor(byte[] baseKey, int objectNumber, int generationNumber) => 
            CreateDecryptor(ObjectEncryptionKey(baseKey, objectNumber, generationNumber));

        private static Span<byte> ObjectEncryptionKey(byte[] baseKey, int objectNumber, int generationNumber) =>
            ComputeHash(baseKey, objectNumber, generationNumber)
                .AsSpan()[..EncryptionKeyLength(baseKey.Length)];

        private static byte[] ComputeHash(byte[] baseKey, int objectNumber, int generationNumber)
        {
            var md5 = MD5.Create();
            md5.AddData(baseKey);
            AddObjectData(objectNumber, generationNumber, md5);
            md5.FinalizeHash();
            return md5.Hash ?? throw new InvalidProgramException("Should have a hash here.");
        }

        private static void AddObjectData(int objectNumber, int generationNumber, MD5 md5) =>
            md5.AddData(new byte[]
            {
                (byte)objectNumber,
                (byte)(objectNumber >> 8),
                (byte)(objectNumber >> 16),
                (byte)generationNumber,
                (byte)(generationNumber >> 8),
            });

        private static int EncryptionKeyLength(int baseKeyLength) => Math.Max(baseKeyLength + 5, 16);

        protected abstract IDecryptor CreateDecryptor(in ReadOnlySpan<byte> encryptionKey);
    }

    public class Rc4DecryptorFactory : DecryptorFactory
    {
        protected override IDecryptor CreateDecryptor(in ReadOnlySpan<byte> encryptionKey) => 
            new Rc4Decryptor(encryptionKey);
    }
    
    public class Rc4Decryptor: IDecryptor
    {
        private RC4 rc4;

        public Rc4Decryptor(in ReadOnlySpan<byte> key)
        {
            rc4 = new RC4(key);
        }
        public void DecryptStringInPlace(in Span<byte> input)
        {
            throw new NotImplementedException();
        }

        public Stream WrapRawStream(Stream input)
        {
            throw new NotImplementedException();
        }
    }
}