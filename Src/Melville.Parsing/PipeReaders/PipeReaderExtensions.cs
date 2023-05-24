using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.PipeReaders;

/// <summary>
/// Special delegate tyoe fir the ReadFrom and TryPars pipereader functions
/// </summary>
/// <typeparam name="T">Target type of the parse operation</typeparam>
/// <param name="reader">The source of data to be parsed.</param>
/// <param name="result">Out parameter for the result of the parsing operation</param>
/// <returns>True if the parsing succeeded, false if there was insufficient data to complete parsing.</returns>
public delegate bool PipeParsingFunc<T>(ref SequenceReader<byte> reader, out T result);

/// <summary>
/// Implements to extension methods on pipereader.
/// </summary>
public static class PipeReaderExtensions
{
    /// <summary>
    /// Repeatedly read from the pipe until there is enough data to successfully parse a value.
    /// It we are at the end of the data return the default value.
    /// </summary>
    /// <typeparam name="T">Return type of the parsing operation</typeparam>
    /// <param name="pipe">The source of the data to be parsed.</param>
    /// <param name="parsingFunc">A delegate that parses the data into the target type</param>
    /// <param name="defaultValue">A default value to return if there is no more data.</param>
    /// <returns>The result of the parsing operation.</returns>
    public static async ValueTask<T> ReadFromAsync<T>(this PipeReader pipe, PipeParsingFunc<T> parsingFunc,T defaultValue)
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