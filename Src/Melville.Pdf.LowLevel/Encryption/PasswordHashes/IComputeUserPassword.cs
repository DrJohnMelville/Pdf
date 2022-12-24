using System;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes;

internal interface IComputeUserPassword
{
    byte[] ComputeHash(in ReadOnlySpan<byte> encryptionKey, EncryptionParameters parameters);
    bool CompareHashes(in ReadOnlySpan<byte> a, in ReadOnlySpan<byte> b);
}