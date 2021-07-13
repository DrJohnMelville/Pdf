using System;
using System.Buffers;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public static class PdfHeaderParser
    {
        public static (bool Success, SequencePosition Position) ParseDocumentHeader(
            ReadResult src, out byte majorVersion, out byte minorVersion)
        {
            var seq = src.Buffer;
            majorVersion = minorVersion = 0;
            var reader = new SequenceReader<byte>(seq);
            if (!reader.TryCheckToken(headerTemplate, out var hasHeader)) return (false, reader.Position);
            if (!hasHeader)
                throw new PdfParseException("File does not begin with a PDF header");
            if (!(reader.TryRead(out var majorByte) &&
                  reader.TryRead(out var periodByte) &&
                  reader.TryRead(out var minorByte))) return (false, reader.Position);
            if (!(CheckByte(majorByte, out majorVersion) && periodByte == '.' &&
                  CheckByte(minorByte, out minorVersion)))
                throw new PdfParseException("Invalid pdf version number.");
            return (true, reader.Position);
        }

        private static bool VerifyHeadder(ref ReadOnlySpan<byte> header, out byte major, out byte minor)
        {
            major = minor = 0;
            if (header.Length < 9) return false;
            if (!header.Slice(0, headerTemplate.Length).SequenceEqual(headerTemplate))
                throw new PdfParseException("File header does not identify it as a pdf file.");
            return CheckByte(header[5], out major) && CheckByte(header[7], out minor);
        }

        private static bool CheckByte(byte b, out byte output)
        {
            output = (byte) (b - '0');
            return output is >= 0 and <= 9;
        }

        private static byte[] headerTemplate = new byte[] {37, 80, 68, 70, 45}; // %PDF-;
    }
}