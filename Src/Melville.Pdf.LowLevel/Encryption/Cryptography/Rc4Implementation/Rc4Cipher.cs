using System;
using System.IO;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;

internal class Rc4Cipher: ICipherOperations, ICipher
{
    private readonly RC4 cryptoImplementation;

    public Rc4Cipher(ReadOnlySpan<byte> finalKey)
    {
        cryptoImplementation = new RC4(finalKey);
    }

    public Span<byte> CryptSpan(Span<byte> input)
    {
        cryptoImplementation.TransfromInPlace(input);
        return input;
    }

    public Stream CryptStream(Stream input) => 
        new Rc4Reader(input.AsReaderInefficient(), cryptoImplementation).AsStreamInefficient();
    public ICipherOperations Encrypt() => this;
    public ICipherOperations Decrypt() => this;
}