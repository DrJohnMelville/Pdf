using System;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes;

internal interface IComputeOwnerPassword
{
    string UserKeyFromOwnerKey(string ownerKey, EncryptionParameters parameters);
    byte[] ComputeOwnerKey(string ownerKey, string userKey, int keyLenInBytes);
}
internal class ComputeOwnerPasswordV2: IComputeOwnerPassword
{
    public string UserKeyFromOwnerKey(string ownerKey, EncryptionParameters parameters) => 
        InnerUserKeyFromOwnerKey(ownerKey, parameters.OwnerPasswordHash, parameters.KeyLengthInBytes);

    private string InnerUserKeyFromOwnerKey(
        string ownerKey, Memory<byte> ownerPasswordHash, int keyLengthInBytes)
    {
        var hash = OwnerPasswordHash(ownerKey);
        var userPass = new byte[ownerPasswordHash.Length];
        ownerPasswordHash.Span.CopyTo(userPass.AsSpan());
        SequentialRc4Encryptor.EncryptDownNTimes(
            hash[..keyLengthInBytes], userPass, SequentialEncryptionCount());
        return BytePadder.PasswordFromBytes(userPass);
    }

    public byte[] ComputeOwnerKey(string ownerKey, string userKey, int keyLenInBytes)
    {
        var ownerHash = OwnerPasswordHash(ownerKey);
        var ret = BytePadder.Pad(userKey);
        SequentialRc4Encryptor.EncryptNTimes(
            ownerHash[..keyLenInBytes], ret, SequentialEncryptionCount());
        return ret;
    }

    protected virtual int SequentialEncryptionCount() => 1;

    private byte[] OwnerPasswordHash(in string ownerPassword)
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