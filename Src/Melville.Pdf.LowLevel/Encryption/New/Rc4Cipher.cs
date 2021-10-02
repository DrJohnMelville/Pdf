using System;
using System.IO;
using Melville.Pdf.LowLevel.Encryption.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption.New
{
    public interface ICipherOperations
    {
        /// <summary>
        /// Encrypt or decrypt a span of bytes.  If the length of plaintext is the same as the length of the
        /// ciphertext , then this function is allowed to do the decryption in place and return the original span
        /// </summary>
        ReadOnlySpan<byte> CryptSpan(Span<byte> input);
        Stream CryptStream(Stream input);
    }

    public interface ICipher
    {
        ICipherOperations Encrypt();
        ICipherOperations Decrypt();
    }
    public class Rc4Cipher: ICipherOperations, ICipher
    {
        private readonly RC4 cryptoImplementation;

        public Rc4Cipher(ReadOnlySpan<byte> finalKey)
        {
            cryptoImplementation = new RC4(finalKey);
        }

        public ReadOnlySpan<byte> CryptSpan(Span<byte> input)
        {
            cryptoImplementation.TransfromInPlace(input);
            return input;
        }

        public Stream CryptStream(Stream input) => new Rc4Stream(input, cryptoImplementation);
        public ICipherOperations Encrypt() => this;
        public ICipherOperations Decrypt() => this;
    }
}