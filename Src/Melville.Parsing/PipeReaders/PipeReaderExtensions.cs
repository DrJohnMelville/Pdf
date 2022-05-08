using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.PipeReaders;

public delegate bool PipeParsingFunc<T>(ref SequenceReader<byte> reader, out T result);

public static class PipeReaderExtensions
{
    public static async ValueTask<T> ReadFrom<T>(this PipeReader pipe, PipeParsingFunc<T> parsingFunc,T defaultValue)
    {
        while (true)
        {
            while (true)
            {
                var result = await pipe.ReadAsync().CA();
                if (TryParse(pipe, new SequenceReader<byte>(result.Buffer), parsingFunc, out var segment)) 
                    return segment;
                if (result.IsCompleted) return defaultValue;
            }

        }
    }

    private static bool TryParse<T>(
        PipeReader pipe, SequenceReader<byte> buffer, PipeParsingFunc<T> parsingFunc, out T result)
    {
        if (parsingFunc(ref buffer, out result))
        {
            pipe.AdvanceTo(buffer.Position);
            return true;
        }
        pipe.AdvanceTo(buffer.Sequence.Start, buffer.Sequence.End);
        return false;

    }
}