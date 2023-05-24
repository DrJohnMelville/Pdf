using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal static class FileTrailerLocater
{
    public static async Task<long> SearchAsync(ParsingFileOwner source, int fileTrailerSizeHint)
    {
        var end = source.StreamLength;
        var start = end;
            
        long xrefPosition = 0;
        while (true)
        {
            (start, end) = ComputeSearchSegment(fileTrailerSizeHint, start, end);
            var context = await source.RentReaderAsync(start).CA();
            var reader = context.Reader;
            while (SearchForS(await reader.ReadAsync().CA(), reader, end, out var foundPos))
            {
                if (!foundPos) continue;
                if (await TokenChecker.CheckTokenAsync(context.Reader, StartXRef).CA())
                {
                    await NextTokenFinder.SkipToNextTokenAsync(reader).CA();
                    do { } while (reader.ShouldContinue(GetLong(await reader.ReadAsync().CA(), out xrefPosition)));
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
        

    private static bool SearchForS(ReadResult readResult, IByteSourceWithGlobalPosition source, long max, out bool foundOne)
    {
        if (readResult.IsCompleted || source.GlobalPosition > max)
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

    private static readonly byte[] StartXRef = "tartxref"u8.ToArray(); 

    private static (bool Success, SequencePosition Position) VerifyTag(
        ReadResult source, byte[] tag, out bool validPos)
    {
        var reader = new SequenceReader<byte>(source.Buffer);
        return !reader.TryCheckToken(tag, source.IsCompleted, out validPos) ? 
            (false, reader.Position) : 
            (true, reader.Position);
    }

}