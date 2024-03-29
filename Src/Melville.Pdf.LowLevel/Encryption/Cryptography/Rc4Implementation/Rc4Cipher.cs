﻿using System;
using System.IO;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

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

    public Stream CryptStream(Stream input) => new Rc4Stream(input, cryptoImplementation);
    public ICipherOperations Encrypt() => this;
    public ICipherOperations Decrypt() => this;
}