using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal static class FieldParser
{
    public static void Read(ref SequenceReader<byte> reader, out uint output) =>
        output = reader.ReadBigEndianUint32();
    public static void Read(ref SequenceReader<byte> reader, out ushort output) =>
        output = reader.ReadBigEndianUint16();

    public static async ValueTask ReadAsync(PipeReader reader, uint[] offsets)
    {
        var buffers = await reader.ReadAtLeastAsync(4 * offsets.Length);
        ReadFrom(reader, buffers, offsets);
    }

    private static void ReadFrom(PipeReader reader, ReadResult buffers, uint[] offsets)
    {
        var seqReader = new SequenceReader<byte>(buffers.Buffer);
        for (int i = 0; i < offsets.Length; i++)
        {
            offsets[i] = seqReader.ReadBigEndianUint32();
        }
        reader.AdvanceTo(seqReader.Position);
    }

    public static async ValueTask<T> ReadFromAsync<T>(PipeReader reader) where T : IGeneratedParsable<T>
    {
        var result = await reader.ReadAtLeastAsync(T.StaticSize);
        LoadFrom(reader, result, out T ret); 
        await ret.LoadAsync(reader);
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
            var segReader = await reader.ReadAtLeastAsync(T.StaticSize);
            LoadFrom(reader, segReader, out records[i]);
            await records[i].LoadAsync(reader);
        }
    }
}