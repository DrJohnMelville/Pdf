using Melville.Parsing.CountingReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace Melville.Fonts.SfntParsers.TableParserParts;

internal static class NumberArrayReader
{
    internal static async ValueTask ReadAsBytesAsync<T>(ParsingBuffer reader, T[] offsets) where T : unmanaged
    {
        var byteLength = offsets.Length * GetSize<T>();
        (await reader.TakeBytesAsync(byteLength).CA()).Span
            .CopyTo(MemoryMarshal.Cast<T, byte>(offsets.AsSpan()));
        
    }
    private static unsafe int GetSize<T>() where T : unmanaged => sizeof(T);

    public const string NumberArrayImplementation = """
        public static async ValueTask ReadAsync(ParsingBuffer reader, ~0~[] offsets)
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

        public static void Read(ref SpanParsingBuffer reader, scoped Span<~0~> data)
        {
            var span = global::System.Runtime.InteropServices.MemoryMarshal.Cast<~0~, byte>(data);
            reader.Peek().TryCopyTo(span);
            reader.Advance(span.Length);
            TryReverseEndianness(data);
        }
        """;
}