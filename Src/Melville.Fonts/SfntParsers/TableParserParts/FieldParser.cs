using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal static partial class FieldParser
{
    public static void Read<T>(ref SequenceReader<byte> reader, out T output) 
        where T : IBinaryInteger<T> =>
        reader.TryReadBigEndian(out output);

    [MacroItem("ushort")]
    [MacroItem("short")]
    [MacroItem("uint")]
    [MacroCode(NumberArrayReader.NumberArrayImplementation)]
    public static  ValueTask ReadAsync(PipeReader reader, Memory<byte> offsets) =>
        NumberArrayReader.ReadAsync(reader, offsets);


    public static async ValueTask<T> ReadFromAsync<T>(PipeReader reader) where T : IGeneratedParsable<T>
    {
        var result = await reader.ReadAtLeastAsync(T.StaticSize).CA();
        LoadFrom(reader, result, out T ret); 
        await ret.LoadAsync(reader).CA();
        return ret;
    }
    
    private static void LoadFrom<T>(PipeReader reader, ReadResult readResult, out T result) 
        where T : IGeneratedParsable<T>
    {
        var seqReader = new SequenceReader<byte>(readResult.Buffer);
        result = T.LoadStatic(ref seqReader);
        reader.AdvanceTo(seqReader.Position);
    }

    public static async ValueTask ReadAsync<T>(PipeReader reader, T[] records)
    where T: IGeneratedParsable<T>
    {
        for (int i = 0; i < records.Length; i++)
        {
            var segReader = await reader.ReadAtLeastAsync(T.StaticSize).CA();
            LoadFrom(reader, segReader, out records[i]);
            await records[i].LoadAsync(reader).CA();
        }
    }
}