using System;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;

public class AesCipher: ICipher
{
    private Aes encryptor;
        
    public AesCipher(in ReadOnlySpan<byte> finalKey)
    {
        encryptor = Aes.Create();
        encryptor.KeySize = 8*finalKey.Length;
        encryptor.Key = finalKey.ToArray();
        encryptor.Mode = CipherMode.CBC;
    }

    public ICipherOperations Encrypt() => new AesEncryptor(encryptor);

    public ICipherOperations Decrypt() => new AesDecryptor(encryptor);
}