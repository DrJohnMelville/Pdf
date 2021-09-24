using System;
using System.Diagnostics;
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
        public byte[] UserKeyFromOwnerKey(in ReadOnlySpan<byte> ownerKey, EncryptionParameters parameters) => 
            InnerUserKeyFromOwnerKey(ownerKey, parameters.OwnerPasswordHash, parameters.KeyLengthInBytes);

        private byte[] InnerUserKeyFromOwnerKey(
            ReadOnlySpan<byte> ownerKey, byte[] ownerPasswordHash, int keyLengthInBytes)
        {
            var hash = OwnerPasswordHash(ownerKey);
            var userPass = new byte[ownerPasswordHash.Length];
            ownerPasswordHash.CopyTo(userPass, 0);
            SequentialRc4Encryptor.EncryptDownNTimes(
                hash[..keyLengthInBytes], userPass, SequentialEncryptionCount());
            return userPass;
        }

        public byte[] ComputeOwnerKey(
            in ReadOnlySpan<byte> ownerKey, in ReadOnlySpan<byte> userKey, int keyLenInBytes)
        {
            var ownerHash = OwnerPasswordHash(ownerKey);
            var ret = BytePadder.Pad(userKey);
            SequentialRc4Encryptor.EncryptNTimes(
                ownerHash[..keyLenInBytes], ret, SequentialEncryptionCount());
            VerifyOwnerHashCanCreateUserKey(ownerKey, userKey, keyLenInBytes, ret);
            return ret;
        }
        
        [Conditional("DEBUG")]
        private void VerifyOwnerHashCanCreateUserKey(ReadOnlySpan<byte> ownerKey, ReadOnlySpan<byte> userKey, int keyLenInBytes, byte[] ret)
        {
            var backCompute = InnerUserKeyFromOwnerKey(ownerKey, ret, keyLenInBytes);
            var paddedUser = BytePadder.Pad(userKey);
            for (int i = 0; i < backCompute.Length; i++)
            {
                if (backCompute[i] != paddedUser[i])
                    throw new InvalidProgramException("Owner key is not invertable");
            }
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
}