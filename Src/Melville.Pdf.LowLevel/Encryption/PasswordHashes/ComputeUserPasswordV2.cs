using System;
using Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes;

internal sealed class ComputeUserPasswordV2 : IComputeUserPassword
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