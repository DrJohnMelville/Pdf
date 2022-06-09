using System;
using System.IO;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;

public class AesDecryptor: ICipherOperations
{
    private readonly Aes encryptor;

    public AesDecryptor(Aes encryptor)
    {
        this.encryptor = encryptor;
    }

    public byte[] CryptSpan(byte[] input)
    {
        var blockSizeInBytes = encryptor.BlockSize / 8;
        encryptor.IV = input[..blockSizeInBytes];
        using var ms = new MemoryStream();
        using var cryptStream = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write);
        cryptStream.Write(input.AsSpan(blockSizeInBytes));
        cryptStream.FlushFinalBlock();
        return ms.ToArray();
    }

    public Stream CryptStream(Stream input) => new AesDecodeStream(input, encryptor);
}