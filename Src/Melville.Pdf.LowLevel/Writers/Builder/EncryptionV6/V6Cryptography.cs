using System;
using System.Security.Cryptography;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

public readonly struct V6Cryptography
{
    public HashAlgorithm Sha256 { get; } = SHA256.Create();
    public HashAlgorithm Sha384 { get; } = SHA384.Create();
    public HashAlgorithm Sha512 { get; } = SHA512.Create();
    public Aes Aes { get; } = Aes.Create();

    public V6Cryptography()
    {
    }

    public Span<byte> Hash(int algorithm, scoped Span<byte> input, Span<byte> output)
    {
        HashFromModulo(algorithm).TryComputeHash(input, output, out var bytesWritten);
        return output[..bytesWritten];
    }
    public HashAlgorithm HashFromModulo(int algorithm) => algorithm switch
    {
        0 => Sha256,
        1 => Sha384,
        2 => Sha512,
        _=> throw new ArgumentOutOfRangeException(nameof(algorithm), "Invalid algorithm")
    };

    public EncryptionStrategy Ecb => new(Aes, EncryptedLengthEcb.Instance, EncryptEcb.Instance, DecryptEcb.Instance);
    public EncryptionStrategy Cbc => new(Aes, EncryptedLengthCbc.Instance, EncryptCbc.Instance, DecryptCbc.Instance);
    
}
