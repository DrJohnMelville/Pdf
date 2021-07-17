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
    public static class FileTrailerLocater
    {
        public static async Task<long> Search(ParsingFileOwner source, int fileTrailerSizeHint)
        {
            var end = source.StreamLength;
            var start = end;
            
            long xrefPosition = 0;
            while (true)
            {
                (start, end) = ComputeSearchSegment(fileTrailerSizeHint, start, end);
                using var context = await source.RentReader(start);
                while (SearchForS(await context.ReadAsync(), context, end, out var foundPos))
                {
                    if (!foundPos) continue;
                    if (await TokenChecker.CheckToken(context, startXRef))
                    {
                        await NextTokenFinder.SkipToNextToken(context);
                        do { } while (context.ShouldContinue(
                            GetLong(await context.ReadAsync(), out xrefPosition)));
                        return xrefPosition;
                    }
                }
            }
        }

        private static (long start, long end) ComputeSearchSegment(int fileTrailerSizeHint, long start, long end)
        {
            if (start <= 0)
                throw new PdfParseException("Could not find trailer");
            (start, end) = (Math.Max(0, end - fileTrailerSizeHint), start);
            return (start, end);
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

        private static (bool Success, SequencePosition Position) VerifyTag(
            ReadResult source, byte[] tag, out bool validPos)
        {
            var reader = new SequenceReader<byte>(source.Buffer);
            return !reader.TryCheckToken(tag, out validPos) ? 
                (false, reader.Position) : 
                (true, reader.Position);
        }

    }
}