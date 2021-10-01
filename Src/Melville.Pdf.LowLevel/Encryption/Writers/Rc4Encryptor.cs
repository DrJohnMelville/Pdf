using System;
using System.IO;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Encryption.Cryptography;
using Melville.Pdf.LowLevel.Encryption.Readers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Encryption.Writers
{
    public class Rc4Encryptor: IObjectEncryptor
    {
        private readonly RC4 rc4;
        public Rc4Encryptor(in ReadOnlySpan<byte> key)
        {
            rc4 = new RC4(key);
        }

        public ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input)
        {
            var output = new byte[input.Length];
            rc4.Transform(input, output);
            return output;
        }

        public Stream WrapReadingStreamWithEncryption(Stream writableStream, PdfName encryptionAlg) =>
            WrapReadingStreamWithEncryption(writableStream);
        public Stream WrapReadingStreamWithEncryption(Stream writableStream) =>
            new Rc4Stream(writableStream, rc4);
    }
}