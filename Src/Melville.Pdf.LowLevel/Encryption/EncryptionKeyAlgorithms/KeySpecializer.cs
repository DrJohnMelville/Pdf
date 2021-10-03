using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms
{
    public interface IKeySpecializer
    {
        ReadOnlySpan<byte> ComputeKeyForObject(byte[] rootKey, int objectNumber, int generationNumber);
    }

    public class Rc4KeySpecializer: IKeySpecializer
    {
        public ReadOnlySpan<byte> ComputeKeyForObject(byte[] rootKey, int objectNumber, int generationNumber)
        {
            Debug.Assert(objectNumber > 0); // test for a former buggy implementation
            var span = ComputeHash(rootKey, objectNumber, generationNumber).AsSpan();
            return span[..EncryptionKeyLength(rootKey.Length)];
        }

        private byte[] ComputeHash(byte[] rootKey, int objectNumber, int generationNumber)
        {
            var md5 = MD5.Create();
            md5.AddData(rootKey);
            AddObjectData(objectNumber, generationNumber, md5);
            md5.FinalizeHash();
            return md5.Hash ?? throw new InvalidProgramException("Should have a hash here.");
        }

        protected virtual void AddObjectData(int objectNumber, int generationNumber, MD5 md5) =>
            md5.AddData(new[]
            {
                (byte)objectNumber,
                (byte)(objectNumber >> 8),
                (byte)(objectNumber >> 16),
                (byte)generationNumber,
                (byte)(generationNumber >> 8),
            });

        private static int EncryptionKeyLength(int baseKeyLength) => Math.Min(baseKeyLength + 5, 16);
    }

    public class AesKeySpecializer: Rc4KeySpecializer
    {
        private static readonly byte[] aesSalt = {0x73,0x41,0x6c,0x54 };
        protected override void AddObjectData(int objectNumber, int generationNumber, MD5 md5)
        {
            base.AddObjectData(objectNumber, generationNumber, md5);
            md5.AddData(aesSalt);
        }
    }
}