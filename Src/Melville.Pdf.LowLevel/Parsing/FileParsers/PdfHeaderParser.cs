using System;
using System.Buffers;
using System.IO.Pipelines;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public static class PdfHeaderParser
    {
        public static (bool Success, SequencePosition Position) ParseDocumentHeader(
            ReadResult src, out byte majorVersion, out byte minorVersion)
        {
            var seq = src.Buffer;
            var reader = new SequenceReader<byte>(seq);
            if (reader.TryReadTo(out ReadOnlySpan<byte> header, (byte) '\n', false) &&
                VerifyHeadder(ref header, out majorVersion, out minorVersion))
            {
                return (true, reader.Position);
            }

            majorVersion = minorVersion = 0;
            return (false, seq.Start);
        }

        private static bool VerifyHeadder(ref ReadOnlySpan<byte> header, out byte major, out byte minor)
        {
            major = minor = 0;
            if (header.Length < 9) return false;
            if (!header.Slice(0, headerTemplate.Length).SequenceEqual(headerTemplate)) return false;
            return CheckByte(header[5], out major) && CheckByte(header[7], out minor);
        }

        private static bool CheckByte(byte b, out byte output)
        {
            output =(byte) (b - '0');
            return output is >= 0 and <= 9;
        }

        private static byte[] headerTemplate = new byte[]{37, 80, 68, 70, 45}; // %PDF-;

    }
}