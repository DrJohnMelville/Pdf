﻿using System;
using System.IO;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

internal interface IObjectCryptContext
{
    public ICipher StringCipher();
    public ICipher StreamCipher();
    public ICipher NamedCipher(in PdfDirectObject name);
        
}
    
internal interface ICipherOperations
{
    /// <summary>
    /// Encrypt or decrypt a span of bytes.  If the length of plaintext is the same as the length of the
    /// ciphertext , then this function is allowed to do the decryption in place and return the original span
    /// </summary>
    Span<byte> CryptSpan(Span<byte> input);
    Stream CryptStream(Stream input);
}

internal interface ICipher
{
    ICipherOperations Encrypt();
    ICipherOperations Decrypt();
}

internal class ErrorObjectEncryptor: IObjectCryptContext
{
    private ErrorObjectEncryptor() { }
    public static IObjectCryptContext Instance { get; } = new ErrorObjectEncryptor();
    public ICipher StringCipher()=> 
        throw new NotSupportedException("Should not be encrypting in this context.");
    public ICipher StreamCipher()=> 
        throw new NotSupportedException("Should not be encrypting in this context.");
    public ICipher NamedCipher(in PdfDirectObject name)=> 
        throw new NotSupportedException("Should not be encrypting in this context.");
}