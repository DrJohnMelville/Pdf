using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using Melville.Parsing.AwaitConfiguration;
using Microsoft.CodeAnalysis;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal static class NumberArrayReader
{
    public static async ValueTask ReadAsync(PipeReader reader, Memory<byte> offsets)
    {
        var buffers = await reader.ReadAtLeastAsync(offsets.Length).CA();
        ReadFrom(reader, buffers, offsets.Span);
    }

    private static void ReadFrom(PipeReader reader, ReadResult buffers, Span<byte> offsets)
    {
        var seqReader = new SequenceReader<byte>(buffers.Buffer);
        seqReader.TryCopyTo(offsets);
        seqReader.Advance(offsets.Length);
        reader.AdvanceTo(seqReader.Position);
    }

    internal static async ValueTask ReadAsBytesAsync<T>(PipeReader reader, T[] offsets) where T : unmanaged
    {
        var buffers = await reader.ReadAtLeastAsync(offsets.Length * GetSize<T>()).CA();
        ReadFrom(reader, buffers, MemoryMarshal.Cast<T, byte>(offsets.AsSpan()));
    }
    private static unsafe int GetSize<T>() where T : unmanaged => sizeof(T);

    public const string NumberArrayImplementation = """
        public static async ValueTask ReadAsync(PipeReader reader, ~0~[] offsets)
        {
            await NumberArrayReader.ReadAsBytesAsync(reader, offsets).CA();
            TryReverseEndianness(offsets);
        }
        private static void TryReverseEndianness(Span<~0~> data)
        {
            if (global::System.BitConverter.IsLittleEndian)
                BinaryPrimitives.ReverseEndianness(data, data);
        }
        
        public static void Read(ref SequenceReader<byte> reader, scoped Span<~0~> data)
        {
            var span = global::System.Runtime.InteropServices.MemoryMarshal.Cast<~0~, byte>(data);
            reader.TryCopyTo(span);
            reader.Advance(span.Length);
            TryReverseEndianness(data);
        }
        """;
}