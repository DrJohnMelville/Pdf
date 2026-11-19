using System;
using System.IO;
using System.Security.Cryptography;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;

internal class AesEncryptor : ICipherOperations
{
    private readonly Aes encryptor;

    public AesEncryptor(Aes encryptor)
    {
        this.encryptor = encryptor;
    }

    public Span<byte> CryptSpan(Span<byte> input)
    {
        var blockLength = encryptor.BlockSize / 8;

        var final = new byte[input.Length + (2 * blockLength)]; // IV plus a possible termination block.
        Span<byte> iv = final.AsSpan(0, blockLength);
        RandomNumberGenerator.Fill(iv);

        var length = encryptor.EncryptCbc(input, iv, final.AsSpan(blockLength));
        return final.AsSpan(0, blockLength + length);
    }

    public Stream CryptStream(Stream input)
    {
        return new ConcatStream(
            new MemoryStream(encryptor.IV),
            new CryptoStream(input, encryptor.CreateEncryptor(), CryptoStreamMode.Read)
        );
    }
}