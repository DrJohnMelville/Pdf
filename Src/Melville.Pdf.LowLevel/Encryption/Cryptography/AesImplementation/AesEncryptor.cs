using System.IO;
using System.Security.Cryptography;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;

public class AesEncryptor : ICipherOperations
{
    private readonly Aes encryptor;

    public AesEncryptor(Aes encryptor)
    {
        this.encryptor = encryptor;
    }

    public byte[] CryptSpan(byte[] input)
    {
        using var ms = new MemoryStream();
        ms.Write(encryptor.IV);
        using var cryptStream = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write);
        cryptStream.Write(input);
        cryptStream.FlushFinalBlock();
        return ms.ToArray();
    }

    public Stream CryptStream(Stream input)
    {
        return new ConcatStream(
            new MemoryStream(encryptor.IV),
            new CryptoStream(input, encryptor.CreateEncryptor(), CryptoStreamMode.Read)
        );
    }
}