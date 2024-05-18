using System.Buffers;
using System.IO.Pipelines;

namespace Melville.Fonts.SfntParsers.TableParserParts
{
    internal interface IGeneratedParsable<T> where T:IGeneratedParsable<T>
    {
        static abstract int StaticSize { get; }
        static abstract T LoadStatic(ref SequenceReader<byte> source);
        ValueTask LoadAsync(PipeReader reader);
    }
}