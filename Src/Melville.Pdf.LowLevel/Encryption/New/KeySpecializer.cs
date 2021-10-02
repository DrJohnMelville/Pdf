using System;
using System.Diagnostics;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Encryption.Readers;

namespace Melville.Pdf.LowLevel.Encryption.New
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

        private static byte[] ComputeHash(byte[] rootKey, int objectNumber, int generationNumber)
        {
            var md5 = MD5.Create();
            md5.AddData(rootKey);
            AddObjectData(objectNumber, generationNumber, md5);
            md5.FinalizeHash();
            return md5.Hash ?? throw new InvalidProgramException("Should have a hash here.");
        }

        private static void AddObjectData(int objectNumber, int generationNumber, MD5 md5) =>
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
}