using System;
using System.IO;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing
{
    public interface IObjectEncryptor
    {
        ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input);
        Stream WrapReadingStreamWithEncryption(Stream stream);
    }
    
    public class NullObjectEncryptor : IObjectEncryptor
    {
        public static readonly NullObjectEncryptor Instance = new();
        private NullObjectEncryptor() { }
        public ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input) => input;

        public Stream WrapReadingStreamWithEncryption(Stream stream) => stream;
    }

}