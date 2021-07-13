using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public class SuggestTrailerPosition
    {
        private readonly ParsingSource context;
        private readonly long startPosition;
        private readonly long fileTrailerSizeHint;
        private long lastRecommendation;

        public SuggestTrailerPosition(ParsingSource context, long fileTrailerSizeHint)
        {
            this.context = context;
            this.fileTrailerSizeHint = fileTrailerSizeHint;
            startPosition = context.Position;
            lastRecommendation = context.StreamLength;
        }

        public long SeekToNextSearchSegment()
        {
            if (lastRecommendation == startPosition)
                throw new PdfParseException("Could not find trailer");
            var endOfSearchSegment = lastRecommendation;
            lastRecommendation = Math.Max(startPosition, lastRecommendation - fileTrailerSizeHint);
            context.Seek(lastRecommendation);
            return endOfSearchSegment;
        }
    }
    public static class ParseTrailer
    {
        public static async Task<PdfDictionary> Parse(ParsingSource context, int fileTrailerSizeHint)
        {
            var trailerPositionGuesser = new SuggestTrailerPosition(context, fileTrailerSizeHint);
            
            long trailerPosition = 0;
            do
            {
                var max = trailerPositionGuesser.SeekToNextSearchSegment();   
                while (SearchForT(await context.ReadAsync(), context, max, out var foundPos))
                {
                    if (!foundPos) continue;
                    bool validTag;
                    do
                    {
                    } while (context.ShouldContinue(
                        TryParseTrailer(await context.ReadAsync(), out validTag)));
                    if (validTag)
                    {
                        trailerPosition = context.Position;
                    }
                }
            } while (trailerPosition == 0);

            
            context.Seek(trailerPosition);
            var possibleDictionary = await context.RootObjectParser.ParseAsync(context);
            if(possibleDictionary is PdfDictionary dictionary) return dictionary;
            throw new PdfParseException("Invalid trailer dictionary");
        }


        private static bool SearchForT(ReadResult readResult, ParsingSource source, long max, out bool foundOne)
        {
            if (readResult.IsCompleted || source.Position > max)
            {
                foundOne = false;
                return false;
            }
            var reader = new SequenceReader<byte>(readResult.Buffer);
            if (reader.TryAdvanceTo((byte) 't'))
            {
                source.AdvanceTo(reader.Position);
                foundOne = true;
                return true;
            }
            

            foundOne = false;
            source.AdvanceTo(readResult.Buffer.End);
            return true;
        }
        public static readonly byte[] trailerTag = {114, 97, 105, 108, 101, 114}; // railer
        private static (bool Success, SequencePosition Position) TryParseTrailer(
            ReadResult source, out bool validPos)
        {
            var reader = new SequenceReader<byte>(source.Buffer);
            return !reader.TryCheckToken(trailerTag, out validPos) ? 
                (false, reader.Position) : 
                (true, reader.Position);
        }
    }
}