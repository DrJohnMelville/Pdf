using System;
using System.Security.Cryptography;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

public readonly partial struct EncryptionStrategy
{
    [FromConstructor] private readonly Aes aesImplementation;
    [FromConstructor]private readonly IEncryptedLength lengthStrategy;
    [FromConstructor]private readonly IEncryptionTransform encryptor;
    [FromConstructor]private readonly IEncryptionTransform decryptor;

    public int CipherLength(int messageLength) => lengthStrategy.CipherLength(aesImplementation, messageLength);
    public Span<byte> Encrypt(Span<Byte> key, Span<byte> iv, Span<byte> source, Span<byte> destination) =>
        EncryptOrDecrypt(key, iv, source, destination, encryptor);
    public Span<byte> Decrypt(Span<Byte> key, Span<byte> iv, Span<byte> source, Span<byte> destination) =>
        EncryptOrDecrypt(key, iv, source, destination, decryptor);

    private Span<byte> EncryptOrDecrypt(Span<Byte> key, Span<byte> iv, Span<byte> source, Span<byte> destination,
        IEncryptionTransform cryptStrategy)
    {
        aesImplementation.Key = key.ToArray();
        var length = cryptStrategy.Transform(aesImplementation, source, iv, destination);
        return destination[..length];
    }
    
}

public interface IEncryptedLength
{
    int CipherLength(Aes aes, int messageLength);
}
[StaticSingleton]
public partial class EncryptedLengthEcb: IEncryptedLength
{
    public int CipherLength(Aes aes, int messageLength) => aes.GetCiphertextLengthEcb(messageLength, PaddingMode.None);
}
[StaticSingleton]
public partial class EncryptedLengthCbc: IEncryptedLength
{
    public int CipherLength(Aes aes, int messageLength) => aes.GetCiphertextLengthCbc(messageLength, PaddingMode.None);
}

public interface IEncryptionTransform
{
    int Transform(Aes aes, Span<byte> source, Span<byte> iv, Span<byte> destination);
}

[StaticSingleton]
public partial class EncryptEcb : IEncryptionTransform
{
    public int Transform(Aes aes, Span<byte> source, Span<byte> iv, Span<byte> destination) =>
        aes.EncryptEcb(source, destination, PaddingMode.None);
} 
[StaticSingleton]
public partial class DecryptEcb : IEncryptionTransform
{
    public int Transform(Aes aes, Span<byte> source, Span<byte> iv, Span<byte> destination) =>
        aes.DecryptEcb(source, destination, PaddingMode.None);
} 
[StaticSingleton]
public partial class EncryptCbc : IEncryptionTransform
{
    public int Transform(Aes aes, Span<byte> source, Span<byte> iv, Span<byte> destination) =>
        aes.EncryptCbc(source, iv, destination, PaddingMode.None);
} 
[StaticSingleton]
public partial class DecryptCbc : IEncryptionTransform
{
    public int Transform(Aes aes, Span<byte> source, Span<byte> iv, Span<byte> destination) =>
        aes.DecryptCbc(source, iv, destination, PaddingMode.None);
} 