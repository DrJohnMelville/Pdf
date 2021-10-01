using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Parsing.Decryptors
{
    public interface IDecryptor
    {
        void DecryptStringInPlace(PdfString input);
        Stream WrapRawStream(Stream input);
        Stream WrapRawStream(Stream input, PdfName cryptFilterName);
    }
    
    
    public class NullDecryptor : IDecryptor
    {
        public static NullDecryptor Instance = new();
        private NullDecryptor(){}
        public void DecryptStringInPlace(PdfString input)
        {
            ; // do nothing
        }

        public Stream WrapRawStream(Stream input, PdfName cryptFilterName) => input;
        public Stream WrapRawStream(Stream input) => input;
    }

}