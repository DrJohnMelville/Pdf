using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public static class RandomAccessFileParser
    {
        public static async Task<PdfLowLevelDocument> Parse(
            ParsingSource context, int fileTrailerSizeHint = 1024)
        {
            if (context.Position != 0)
                throw new PdfParseException("Parsing must begin at position 0.");
            byte major, minor;
            do { } while (context.ShouldContinue(
                PdfHeaderParser.ParseDocumentHeader(await context.ReadAsync(), out major, out minor)));
            
            var dictionary = await ParseTrailer.Parse(context, fileTrailerSizeHint);
            
            long xrefPos;
            do {} while (context.ShouldContinue(FindXrePosition(await context.ReadAsync(), out xrefPos))) ;
            context.Seek(xrefPos);
            await new CrossReferenceTableParser(context).Parse();

            return new PdfLowLevelDocument(major, minor, dictionary, context.IndirectResolver.GetObjects());
        }
        
        private static readonly byte[] startXRef = 
            {115, 116, 97, 114, 116, 120, 114, 101, 102}; // startxref
        private static (bool Success, SequencePosition Position) 
            FindXrePosition(ReadResult source, out long xrefPos)
        {
            xrefPos = -1;
            var reader = new SequenceReader<byte>(source.Buffer);
            if (!NextTokenFinder.SkipToNextToken(ref reader)) return (false, reader.Position);
            if (!reader.TryCheckToken(startXRef, out var hasStartRef))
                return (false, reader.Position);
            if (!hasStartRef) throw new PdfParseException("StatXRef expected");
            if (!NextTokenFinder.SkipToNextToken(ref reader)) return (false, reader.Position);
            if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out xrefPos, out _))
                return (false, reader.Position);
            return (true, reader.Position);

        }
    }
}