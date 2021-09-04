using System;
using System.IO;

namespace Melville.Pdf.LowLevel.Parsing.Decryptors
{
    public interface IDecryptor
    {
#warning this will not work with AES, which pads strings
        void DecryptStringInPlace(in Span<byte> input);
        Stream WrapRawStream(Stream input);
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