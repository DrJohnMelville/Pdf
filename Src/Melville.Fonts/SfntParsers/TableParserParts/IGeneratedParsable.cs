using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.CountingReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal interface IGeneratedParsable<T> where T:IGeneratedParsable<T>
{
    static abstract int StaticSize { get; }
    static abstract T LoadStatic(ref SpanParsingBuffer source);
    ValueTask LoadAsync(ParsingBuffer reader);
}

internal static class GeneratedParsableExtensions
{
    public static int GetStaticSize<T>(this T item)  where T:IGeneratedParsable<T> => 
        T.StaticSize;
}