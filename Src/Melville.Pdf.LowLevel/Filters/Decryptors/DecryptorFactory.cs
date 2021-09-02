using System;
using System.IO;

namespace Melville.Pdf.LowLevel.Filters.Decryptors
{
    public interface IDecryptorFactory
    {
        IDecryptor CreateFor(int objectNumber, int generationNumber);
    }

    public interface IDecryptor
    {
        void DecryptStringInPlace(in Span<byte> input);
        Stream WrapRawStream(Stream input);
    }

    public class NullDecryptorFactory : IDecryptorFactory
    {
        public static NullDecryptorFactory Instance = new();

        private NullDecryptorFactory() { }

        public IDecryptor CreateFor(int objectNumber, int generationNumber) => NullDecryptor.Instance;
    }
    public class NullDecryptor : IDecryptor
    {
        public static NullDecryptor Instance = new();
        private NullDecryptor(){}
        public void DecryptStringInPlace(in Span<byte> input)
        {
            ; // do nothing
        }
        public Stream WrapRawStream(Stream input) => input;
    }
}