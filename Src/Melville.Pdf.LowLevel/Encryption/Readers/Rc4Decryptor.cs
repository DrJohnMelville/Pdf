using System;
using System.IO;
using Melville.Pdf.LowLevel.Encryption.Cryptography;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public class Rc4Decryptor: IDecryptor
    {
        private readonly RC4 rc4;

        public Rc4Decryptor(in ReadOnlySpan<byte> key)
        {
            rc4 = new RC4(key);
        }
        
        public void DecryptStringInPlace(PdfString input) => rc4.TransfromInPlace(input.Bytes);
        public Stream WrapRawStream(Stream input, PdfName cryptFilterName) => new Rc4Stream(input, rc4);
        public Stream WrapRawStream(Stream input) => new Rc4Stream(input, rc4);
    }
}