using System;
using System.IO;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;

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
        var ret = new byte[input.Length - blockSizeInBytes];
        var len =encryptor.DecryptCbc(input[blockSizeInBytes..], input[..blockSizeInBytes],
            ret);
        return ret.AsSpan(0, len);
    }

    public Stream CryptStream(Stream input) => new AesDecodeReader(
        input.AsReaderInefficient(), encryptor).AsStreamInefficient();
}