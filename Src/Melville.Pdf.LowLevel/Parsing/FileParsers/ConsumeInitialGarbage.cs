using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

public static class ConsumeInitialGarbage
{
    public static async ValueTask<int> CheckForOffset(IPipeReaderWithPosition context)
    {
        int offset;
        do {} while(context.Source.ShouldContinue(SkipGarbage(await context.Source.ReadAsync().ConfigureAwait(false), out offset)));

        return offset;

    }

    private static (bool Success, SequencePosition Position) SkipGarbage(ReadResult result, out int offset)
    {
        offset = 0;
        var sr = new SequenceReader<byte>(result.Buffer);
        while (true)
        {
            if (!sr.TryPeek(out var item)) return (false, result.Buffer.Start);
            if (item == '%') return (true, sr.Position);
            //otherwise
            offset++;
            sr.Advance(1);
        }
    }
}