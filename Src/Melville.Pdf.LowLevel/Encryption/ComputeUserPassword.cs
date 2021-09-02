using System;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Encryption.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface IComputeUserPassword
    {
        byte[] ComputeHash(in ReadOnlySpan<byte> encryptionKey, EncryptionParameters parameters);
        bool CompareHashes(in ReadOnlySpan<byte> a, in ReadOnlySpan<byte> b);
    }

    public sealed class ComputeUserPasswordV2 : IComputeUserPassword
    {
        public byte[] ComputeHash(in ReadOnlySpan<byte> encryptionKey, EncryptionParameters parameters)
        {
            var rc4 = new RC4(encryptionKey);
            var ret = new byte[32];
            rc4.Transform(BytePadder.PdfPasswordPaddingBytes, ret);
            return ret;
        }

        public bool CompareHashes(in ReadOnlySpan<byte> a, in ReadOnlySpan<byte> b) => 
            a.SequenceCompareTo(b) == 0;
    }

    public sealed class ComputeUserPasswordV3 : IComputeUserPassword
    {
        public byte[] ComputeHash(in ReadOnlySpan<byte> encryptionKey, EncryptionParameters parameters)
        {
            var hash = ComputeInitialHash(parameters);
            Encrypt20Times(encryptionKey, hash);

            return WriteKeyTwice(hash);
        }

        private void Encrypt20Times(ReadOnlySpan<byte> encryptionKey, byte[] hash)
        {
            for (int i = 0; i < 20; i++)
            {
                var loopRc4 = new RC4(RoundKey(encryptionKey, i));
                loopRc4.TransfromInPlace(hash);
            }
        }

        private static byte[] WriteKeyTwice(byte[] hash)
        {
            var ret = new byte[32];
            hash.AsSpan().CopyTo(ret);
            hash.AsSpan().CopyTo(ret.AsSpan(16));
            return ret;
        }

        private static byte[] ComputeInitialHash(EncryptionParameters parameters)
        {
            var md5 = MD5.Create();
            md5.AddData(BytePadder.PdfPasswordPaddingBytes);
            md5.AddData(parameters.IdFirstElement);
            md5.FinalizeHash();
            var hash = md5.Hash;
            return hash;
        }

        private byte[] RoundKey(ReadOnlySpan<byte> encryptionKey, int iteration)
        {
            var ret = new byte[encryptionKey.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (byte) (encryptionKey[i] ^ iteration);
            }

            return ret;
        }

        public bool CompareHashes(in ReadOnlySpan<byte> a, in ReadOnlySpan<byte> b) => 
            a[..16].SequenceCompareTo(b[..16]) == 0;
    }
}