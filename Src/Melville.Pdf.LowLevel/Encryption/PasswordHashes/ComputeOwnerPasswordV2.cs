using System;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Encryption.Readers;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes
{
    public interface IComputeOwnerPassword
    {
        byte[] UserKeyFromOwnerKey(in ReadOnlySpan<byte> ownerKey, EncryptionParameters parameters);

        byte[] ComputeOwnerKey(
            in ReadOnlySpan<byte> ownerKey, in ReadOnlySpan<byte> userKey, int keyLenInBytes);
    }
    public class ComputeOwnerPasswordV2: IComputeOwnerPassword
    {
        public byte[] UserKeyFromOwnerKey(in ReadOnlySpan<byte> ownerKey, EncryptionParameters parameters)
        {
            var hash = OwnerPasswordHash(ownerKey);

            var userPass = new byte[parameters.OwnerPasswordHash.Length];
            parameters.OwnerPasswordHash.CopyTo(userPass, 0);
            SequentialRc4Encryptor.EncryptDownNTimes(hash[..parameters.KeyLengthInBytes], userPass, 20);
            return userPass;
        }

        public byte[] ComputeOwnerKey(
            in ReadOnlySpan<byte> ownerKey, in ReadOnlySpan<byte> userKey, int keyLenInBytes)
        {
            var ownerHash = OwnerPasswordHash(ownerKey);
            var userHash = BytePadder.Pad(userKey);
            SequentialRc4Encryptor.EncryptNTimes(ownerHash[..keyLenInBytes], userHash, 
                SequentialEncryptionCount());
            return userHash;
        }

        protected virtual int SequentialEncryptionCount() => 1;

        private byte[] OwnerPasswordHash(in ReadOnlySpan<byte> ownerPassword)
        {
            Span<byte> paddedPassword = stackalloc byte[32];
            BytePadder.Pad(ownerPassword, paddedPassword);
            return ComputeMd5Hash(paddedPassword);
        }

        protected virtual byte[] ComputeMd5Hash(Span<byte> paddedPassword)
        {
            return MD5.HashData(paddedPassword);
        }

    }

    public class ComputeOwnerPasswordV3 : ComputeOwnerPasswordV2
    {
        protected override int SequentialEncryptionCount() => 20;

        protected override byte[] ComputeMd5Hash(Span<byte> paddedPassword)
        {
            Span<byte> source = stackalloc byte[16];
            Span<byte> dest = stackalloc byte[16];
            MD5.HashData(paddedPassword, source);
            for (int i = 0; i < 25; i++) // unroll the loop by two to avoid swaping the arguments
            {
                MD5.HashData(source, dest);
                MD5.HashData(dest, source);
            }

            return source.ToArray();
        }
    }
}