using Melville.INPC;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal static partial class FieldParser
{

    public static void Read<T>(ref SpanParsingBuffer reader, out T output)
        where T : IBinaryInteger<T> =>
        reader.TryReadBigEndian(out output);

    public static void Read(ref SpanParsingBuffer reader, out UInt24 output) =>
        UInt24.TryReadBigEndian(ref reader, out output);

    public static void Read<TStorage, TCompute, TConfig>(
        ref SpanParsingBuffer reader, out FixedPoint<TStorage, TCompute, TConfig> output) 
        where TStorage : IBinaryInteger<TStorage>
        where TCompute : IBinaryInteger<TCompute>
        where TConfig : IFixedPointConfig<TStorage, TCompute>
    {
        reader.TryReadBigEndian(out TStorage value);
        output = new FixedPoint<TStorage, TCompute, TConfig>(value);
    }


    [MacroItem("ushort", typeof(BinaryPrimitives))]
    [MacroItem("short")]
    [MacroItem("uint")]
    [MacroCode(NumberArrayReader.NumberArrayImplementation)]
    public static  ValueTask ReadAsync(ParsingBuffer reader, Memory<byte> offsets) =>
        reader.FillExternalBufferAsync(offsets);


    public static async ValueTask<T> ReadFromAsync<T>(IMultiplexSource source, long offset = 0 ) where T : IGeneratedParsable<T>
    {
        using var buffer = source.ParsingBufferFrom(offset);
        return await ReadFromAsync<T>(buffer);
    }



    public static async ValueTask<T> ReadFromAsync<T>(ParsingBuffer reader) where T : IGeneratedParsable<T>
    {
        var block = await reader.TakeBytesAsync(T.StaticSize);
        var spanReader = new SpanParsingBuffer(block.Span);
        var ret = T.LoadStatic(ref spanReader);
        await ret.LoadAsync(reader).CA();
        return ret;
    }
 
    public static async ValueTask ReadAsync<T>(ParsingBuffer reader, T[] records)
    where T: IGeneratedParsable<T>
    {
        for (int i = 0; i < records.Length; i++)
        {
            records[i] = await ReadFromAsync<T>(reader).CA();
        }
    }
}