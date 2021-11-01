using System;
using System.IO;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

public interface IObjectCryptContext
{
    public ICipher StringCipher();
    public ICipher StreamCipher();
    public ICipher NamedCipher(PdfName name);
        
}
    
public interface ICipherOperations
{
    /// <summary>
    /// Encrypt or decrypt a span of bytes.  If the length of plaintext is the same as the length of the
    /// ciphertext , then this function is allowed to do the decryption in place and return the original span
    /// </summary>
    byte[] CryptSpan(byte[] input);
    Stream CryptStream(Stream input);
}

public interface ICipher
{
    ICipherOperations Encrypt();
    ICipherOperations Decrypt();
}

public class ErrorObjectEncryptor: IObjectCryptContext
{
    private ErrorObjectEncryptor() { }
    public static IObjectCryptContext Instance { get; } = new ErrorObjectEncryptor();
    public ICipher StringCipher()=> 
        throw new NotSupportedException("Should not be encrypting in this context.");
    public ICipher StreamCipher()=> 
        throw new NotSupportedException("Should not be encrypting in this context.");
    public ICipher NamedCipher(PdfName name)=> 
        throw new NotSupportedException("Should not be encrypting in this context.");
}