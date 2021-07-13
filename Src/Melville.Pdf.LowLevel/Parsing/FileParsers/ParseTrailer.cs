using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public static class ParseTrailer
    {
        public static async Task<PdfDictionary> Parse(ParsingSource context, int fileTrailerSizeHint)
        {
            long trailerPosition = 0;
            while (SearchForT(await context.ReadAsync(), context, out var foundPos))
            {
                if (!foundPos) continue;
                bool validTag = false;
                do { } while (context.ShouldContinue(
                    TryParseTrailer(await context.ReadAsync(), out validTag)));

                if (validTag)
                {
                    trailerPosition = context.Position;
                }
            }

            if (trailerPosition == 0) throw new PdfParseException("Could not find trailer");
            context.Seek(trailerPosition);
            var dictionary = (PdfDictionary)await context.RootObjectParser.ParseAsync(context);

            return dictionary;
        }


        private static bool SearchForT(ReadResult readResult, ParsingSource source, out bool foundOne)
        {
            if (readResult.IsCompleted)
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