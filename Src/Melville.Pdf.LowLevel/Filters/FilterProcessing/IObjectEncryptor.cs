using System;
using System.IO;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing
{
    public interface IObjectEncryptor
    {
        ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input);
        Stream WrapReadingStreamWithEncryption(Stream stream);
        Stream WrapReadingStreamWithEncryption(Stream stream, PdfName encryptionAlg);
    }
    
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


    
    
    public class NullObjectEncryptor : IObjectEncryptor
    {
        public static readonly NullObjectEncryptor Instance = new();
        private NullObjectEncryptor() { }
        public ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input) => input;

        public Stream WrapReadingStreamWithEncryption(Stream stream) => stream;
        public Stream WrapReadingStreamWithEncryption(Stream stream, PdfName encryptionAlg) => stream;
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
}