using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers.V6SecurityHandler;

public partial class RootKeyComputerV6 : IRootKeyComputer
{
    [FromConstructor] private V6EncryptionKey userKey;
    [FromConstructor] private V6EncryptionKey ownerKey;
    private readonly V6Cryptography crypto = new();
    
    public byte[]? TryComputeRootKey(string password, PasswordType type) =>
        type == PasswordType.User ? 
            CheckKey(userKey, Span<byte>.Empty, password) : 
            CheckKey(ownerKey, userKey.WholeKey, password);

    private byte[]? CheckKey(V6EncryptionKey key, Span<byte> extraBytes, string password) => 
        !CheckPasswordValid(key, extraBytes, password) ? 
            null : ComputeFileEncryptionKey(key, extraBytes, password);

    private bool CheckPasswordValid(V6EncryptionKey key, Span<byte> extraBytes, string password)
    {
        Span<byte> computedHash = stackalloc byte[32];
        HashAlgorithm2B.ComputePasswordHash(password, key.ValidationSalt, extraBytes, computedHash, crypto);
        return key.Hash.SequenceEqual(computedHash);
    }

    private byte[] ComputeFileEncryptionKey(V6EncryptionKey key, Span<byte> extraBytes, string password)
    {
        var fileEncryptionKey = new byte[32];
        HashAlgorithm2B.ComputePasswordHash(password, key.KeySalt, extraBytes, fileEncryptionKey, crypto);
        return fileEncryptionKey;
    }
}