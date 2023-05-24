using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal static class ConsumeInitialGarbage
{
    public static async ValueTask<int> CheckForOffsetAsync(IByteSourceWithGlobalPosition context)
    {
        int offset;
        do {} while(context.ShouldContinue(SkipGarbage(await context.ReadAsync().CA(), out offset)));

        return offset;

    }

    private static (bool Success, SequencePosition Position) SkipGarbage(ReadResult result, out int offset)
    {
        offset = 0;
        var sr = new SequenceReader<byte>(result.Buffer);
        while (true)
        {
            if (!sr.TryPeek(out var item))
            {
                if (result.IsCompleted)
                    throw new PdfParseException("No PDF data found to parse");
                return (false, result.Buffer.Start);
            }
            if (item == '%') return (true, sr.Position);
            //otherwise
            offset++;
            sr.Advance(1);
        }
    }
}