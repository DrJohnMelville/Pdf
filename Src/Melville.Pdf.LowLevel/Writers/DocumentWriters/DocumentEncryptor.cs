using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public interface IDocumentEncryptor
    {
        IObjectEncryptor CreateEncryptor(PdfIndirectObject parentObject, PdfName cryptFilterName);
    }

    public interface IObjectEncryptor
    {
        ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input);
        Stream WrapReadingStreamWithEncryption(Stream stream);
    }

    public class NullDocumentEncryptor: IDocumentEncryptor
    {
        public static readonly NullDocumentEncryptor Instance = new();

        private NullDocumentEncryptor()
        {
        }

        public IObjectEncryptor CreateEncryptor(PdfIndirectObject parentObject, PdfName cryptFilterName) =>
            NullObjectEncryptor.Instance;
    }

    public class NullObjectEncryptor : IObjectEncryptor
    {
        public static readonly NullObjectEncryptor Instance = new();
        private NullObjectEncryptor() { }
        public ReadOnlySpan<byte> Encrypt(in ReadOnlySpan<byte> input) => input;

        public Stream WrapReadingStreamWithEncryption(Stream stream) => stream;
    }
}