using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public class SuggestTrailerPosition
    {
        private readonly long fileTrailerSizeHint;
        private long lastRecommendation;

        public SuggestTrailerPosition(long totalLength, long fileTrailerSizeHint)
        {
            this.fileTrailerSizeHint = fileTrailerSizeHint;
            lastRecommendation = totalLength;
        }

        public (long Start, long End) NextSegment()
        {
            if (lastRecommendation <= 0)
                throw new PdfParseException("Could not find trailer");
            var endOfSearchSegment = lastRecommendation;
            lastRecommendation = Math.Max(0, lastRecommendation - fileTrailerSizeHint);
            return (lastRecommendation, endOfSearchSegment);
        }
    }
    public static class ParseTrailer
    {
        public static async Task<(PdfDictionary Dictionary, long XrefPos)> 
            Parse(ParsingFileOwner source, int fileTrailerSizeHint)
        {
            var trailerPositionGuesser 
                = new SuggestTrailerPosition(source.StreamLength, fileTrailerSizeHint);
            
            long xrefPosition = 0;
            do
            {
                var (start, end) = trailerPositionGuesser.NextSegment();
                using (var context = await source.RentReader(start))
                {
                    while (SearchForS(await context.ReadAsync(), context, end, out var foundPos))
                    {
                        if (!foundPos) continue;
                        bool validTag;
                        do { } while (context.ShouldContinue(
                            verifyTag(await context.ReadAsync(), startXRef, out validTag)));
                        if (validTag)
                        {
                            await NextTokenFinder.SkipToNextToken(context);
                            do
                            {
                            } while (context.ShouldContinue(GetLong(await context.ReadAsync(), out xrefPosition)));
                            
                        }
                    }
                }
            } while (xrefPosition == 0);

            using (var context = await source.RentReader(xrefPosition))
            {
                
                await new CrossReferenceTableParser(context).Parse();
                await NextTokenFinder.SkipToNextToken(context);
                bool validDict;
                do { } while (context.ShouldContinue(
                    verifyTag(await context.ReadAsync(), trailerTag, out validDict)));
                
                if (!validDict) throw new PdfParseException("Trailer does not follow xref");
                var trailer = await context.RootObjectParser.ParseAsync(context);
                if (trailer is not PdfDictionary trailerDictionery)
                    throw new PdfParseException("Trailer dictionary is invalid");
                return (trailerDictionery, xrefPosition);
            }
            // context.Seek(trailerPosition);
            // var possibleDictionary = await context.RootObjectParser.ParseAsync(context);
            // if(possibleDictionary is PdfDictionary dictionary) return dictionary;
            // throw new PdfParseException("Invalid trailer dictionary");
        }

        private static (bool Success, SequencePosition Position)
            GetLong(ReadResult buffer, out long trailerPosition)
        {
            var reader = new SequenceReader<byte>(buffer.Buffer);
            return (WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out trailerPosition, out _),
                reader.Position);
        }
        

        private static bool SearchForS(ReadResult readResult, IParsingReader source, long max, out bool foundOne)
        {
            if (readResult.IsCompleted || source.Position > max)
            {
                foundOne = false;
                return false;
            }
            var reader = new SequenceReader<byte>(readResult.Buffer);
            if (reader.TryAdvanceTo((byte) 's'))
            {
                source.AdvanceTo(reader.Position);
                foundOne = true;
                return true;
            }
            

            foundOne = false;
            source.AdvanceTo(readResult.Buffer.End);
            return true;
        }
        private static readonly byte[] startXRef = 
            {116, 97, 114, 116, 120, 114, 101, 102}; // tartxref

        private static (bool Success, SequencePosition Position) verifyTag(
            ReadResult source, byte[] tag, out bool validPos)
        {
            var reader = new SequenceReader<byte>(source.Buffer);
            return !reader.TryCheckToken(tag, out validPos) ? 
                (false, reader.Position) : 
                (true, reader.Position);
        }

        public static readonly byte[] trailerTag = {(byte)'t',114, 97, 105, 108, 101, 114}; // trailer
    }
}