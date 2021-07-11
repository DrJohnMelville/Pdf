using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public class RandomAccessFileParser
    {
        private readonly ParsingSource context;

        public RandomAccessFileParser(ParsingSource context)
        {
            this.context = context;
        }

        public async Task<PdfLowLevelDocument> Parse()
        {
            CheckBeginAtPositionZero();
            PdfLowLevelDocument? doc;
            do { } while (context.ShouldContinue(ParseDocumentHeader(await context.ReadAsync(), out doc)));

            return doc;
        }

        private (bool Success, SequencePosition Position) ParseDocumentHeader(
            ReadResult src, out PdfLowLevelDocument result)
        {
            var seq = src.Buffer;
            var reader = new SequenceReader<byte>(seq);
            if (reader.TryReadTo(out ReadOnlySpan<byte> header, (byte) '\n', false) &&
                VerifyHeadder(ref header, out var major, out var minor))
            {
                result = new PdfLowLevelDocument(major, minor);
                return (true, reader.Position);
            }

            result = null!;
            return (false, seq.Start);
        }

        private bool VerifyHeadder(ref ReadOnlySpan<byte> header, out byte major, out byte minor)
        {
            major = minor = 0;
            if (header.Length < 9) return false;
            if (!header.Slice(0, headerTemplate.Length).SequenceEqual(headerTemplate)) return false;
            return CheckByte(header[5], out major) && CheckByte(header[7], out minor);
        }

        private bool CheckByte(byte b, out byte output)
        {
            output =(byte) (b - '0');
            return output is >= 0 and <= 9;
        }

        private static byte[] headerTemplate = new byte[]{37, 80, 68, 70, 45}; // %PDF-;
        

        private void CheckBeginAtPositionZero()
        {
            if (context.Position != 0)
                throw new PdfParseException("Parsing must begin at position 0.");
        }
    }
}