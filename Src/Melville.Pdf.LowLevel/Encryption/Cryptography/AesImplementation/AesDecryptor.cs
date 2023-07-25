using System;
using System.IO;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;

internal class AesDecryptor: ICipherOperations
{
    private readonly Aes encryptor;

    public AesDecryptor(Aes encryptor)
    {
        this.encryptor = encryptor;
    }

    public Span<byte> CryptSpan(Span<byte> input)
    {
        var blockSizeInBytes = encryptor.BlockSize / 8;
        #warning -- get rid of the ToArray
        encryptor.IV = input[..blockSizeInBytes].ToArray();
        using var ms = new MemoryStream();
        using var cryptStream = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write);
        cryptStream.Write(input[blockSizeInBytes..]);
        cryptStream.FlushFinalBlock();
        return ms.ToArray();
    }

    public Stream CryptStream(Stream input) => new AesDecodeStream(input, encryptor);
}