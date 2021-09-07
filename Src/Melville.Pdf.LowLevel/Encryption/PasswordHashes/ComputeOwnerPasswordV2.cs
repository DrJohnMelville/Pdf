using System;
using System.Buffers;
using System.Security.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes
{
    public interface IComputeOwnerPassword
    {
        byte[] UserKeyFromOwnerKey(in ReadOnlySpan<byte> ownerKey, EncryptionParameters parameters);
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

        private byte[] ComputeOwnerKey(
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
            var ret = BytePadder.Pad(ownerPassword);
            for (int i = 0; i < Md5IterationCount(); i++)
            {
                #warning see if I can reuse the buffers here
                ret = MD5.HashData(ret);
            }

            return ret;
        }

        protected virtual int Md5IterationCount() => 1;
    }

    public class ComputeOwnerPasswordV3 : ComputeOwnerPasswordV2
    {
        protected override int Md5IterationCount() => 51;
        protected override int SequentialEncryptionCount() => 20;
    }
}