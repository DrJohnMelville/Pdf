using System;
using Melville.Pdf.LowLevel.Encryption.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes
{
    public static class SequentialRc4Encryptor
    {
        public static void EncryptNTimes   (in ReadOnlySpan<byte> encryptionKey, byte[] hash, int interationsDesired)
        {
            DoWork(encryptionKey, hash, 0, interationsDesired, 1);
        }
        public static void EncryptDownNTimes(in ReadOnlySpan<byte> encryptionKey, byte[] hash, int interationsDesired)
        {
            DoWork(encryptionKey, hash, interationsDesired -1, -1, -1);
        }
        private static void DoWork(in ReadOnlySpan<byte> encryptionKey, byte[] hash,
            int first, int terminator, int increment)
        {
            var rc4 = new RC4();
            Span<byte> key = stackalloc byte[encryptionKey.Length];
            for (int i = first; i != terminator; i+= increment)
            {
                RoundKey(encryptionKey, i, key);
                rc4.ResetWithNewKey(key);
                rc4.TransfromInPlace(hash);
            }
        }

        private static void RoundKey(ReadOnlySpan<byte> encryptionKey, int iteration, Span<byte> output)
        {
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = (byte) (encryptionKey[i] ^ iteration);
            }
        }
    }
}