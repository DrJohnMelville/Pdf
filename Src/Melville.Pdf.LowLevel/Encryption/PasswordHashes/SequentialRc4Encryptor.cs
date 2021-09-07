using System;
using Melville.Pdf.LowLevel.Encryption.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes
{
    public static class SequentialRc4Encryptor
    {
        public static void EncryptNTimes   (ReadOnlySpan<byte> encryptionKey, byte[] hash, int interationsDesired)
        {
            for (int i = 0; i < interationsDesired; i++)
            {
                #warning keep the buffer for the key on the stack instead of 20 heap objects
                new RC4(RoundKey(encryptionKey, i)).TransfromInPlace(hash);
            }
        }
        public static void EncryptDownNTimes(ReadOnlySpan<byte> encryptionKey, byte[] hash, int interationsDesired)
        {
            for (int i = interationsDesired-1; i >= 0; i--)
            {
                #warning keep the buffer for the key on the stack instead of 20 heap objects
                new RC4(RoundKey(encryptionKey, i)).TransfromInPlace(hash);
            }
        }

        private static byte[] RoundKey(ReadOnlySpan<byte> encryptionKey, int iteration)
        {
            var ret = new byte[encryptionKey.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (byte) (encryptionKey[i] ^ iteration);
            }

            return ret;
        }
    }
}