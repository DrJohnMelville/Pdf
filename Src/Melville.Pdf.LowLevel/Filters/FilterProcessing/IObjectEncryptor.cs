using System;
using System.IO;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing
{
    public interface IObjectEncryptor
    {
        ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input);
        Stream WrapReadingStreamWithEncryption(Stream stream);
        Stream WrapReadingStreamWithEncryption(Stream stream, PdfName encryptionAlg);
    }
    
    public class NullObjectEncryptor : IObjectEncryptor
    {
        public static readonly NullObjectEncryptor Instance = new();
        private NullObjectEncryptor() { }
        public ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input) => input;

        public Stream WrapReadingStreamWithEncryption(Stream stream) => stream;
        public Stream WrapReadingStreamWithEncryption(Stream stream, PdfName encryptionAlg) => stream;
    }
    
    public class ErrorObjectEncryptor: IObjectEncryptor
    {
        public ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input) => 
            throw new NotSupportedException("Should not be encrypting in this context.");

        public Stream WrapReadingStreamWithEncryption(Stream stream) =>
            throw new NotSupportedException("Should not be encrypting in this context.");
        public Stream WrapReadingStreamWithEncryption(Stream stream, PdfName encryptionAlg) =>
            throw new NotSupportedException("Should not be encrypting in this context.");

        private ErrorObjectEncryptor() { }

        public static IObjectEncryptor Instance { get; } = new ErrorObjectEncryptor();
    }
}