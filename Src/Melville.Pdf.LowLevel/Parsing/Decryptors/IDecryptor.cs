using System;
using System.IO;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Parsing.Decryptors
{
    public interface IDecryptor
    {
        void DecryptStringInPlace(PdfString input);
        Stream WrapRawStream(Stream input, PdfStream targetStream);
    }
    
    
    public class NullDecryptor : IDecryptor
    {
        public static NullDecryptor Instance = new();
        private NullDecryptor(){}
        public void DecryptStringInPlace(PdfString input)
        {
            ; // do nothing
        }
        public Stream WrapRawStream(Stream input, PdfStream targetStream) => input;
    }

}