using System;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes;

internal sealed class ComputeUserPasswordV3 : IComputeUserPassword
{
    public byte[] ComputeHash(in ReadOnlySpan<byte> encryptionKey, EncryptionParameters parameters)
    {
        var hash = ComputeInitialHash(parameters);
        SequentialRc4Encryptor.EncryptNTimes(encryptionKey, hash, 20);

        return WriteKeyTwice(hash);
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
        var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        md5.AppendData(BytePadder.PdfPasswordPaddingBytes);
        md5.AppendData(parameters.IdFirstElement.Span);
        return md5.GetCurrentHash();
    }
        
    public bool CompareHashes(in ReadOnlySpan<byte> a, in ReadOnlySpan<byte> b) => 
        a[..16].SequenceCompareTo(b[..16]) == 0;
}