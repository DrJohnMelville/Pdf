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

    public IPdfStreamReader CryptStream(IPdfStreamReader input) => 
        new Rc4Reader(input, cryptoImplementation);
    public ICipherOperations Encrypt() => this;
    public ICipherOperations Decrypt() => this;
}