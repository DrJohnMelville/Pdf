using Melville.StreamInterfaces;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal readonly struct AsyncReaderToByteArray(IAsyncReader reader, bool rentFinalBuffer = true)
{
    private readonly List<byte[]> Buffers = new();

    public async ValueTask<(byte[], int)> ReadAsync()
    {
        while (true)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            var length = await reader.ReadAtLeastAsync(buffer, buffer.Length, default, false).CA();
            if (length < buffer.Length) return MakeFinalBuffer(buffer, length);
            Buffers.Add(buffer);
        }
    }
    (byte[], int) MakeFinalBuffer(byte[] buffer, int length)
    {
        var totalLen = length + Buffers.Sum(i => i.Length);
        var finalBuffer = rentFinalBuffer ?  
            ArrayPool<byte>.Shared.Rent(totalLen):
                                new byte[totalLen];
        var nextByte = finalBuffer.AsSpan();
        foreach(var buf in Buffers)
        {
            AddBytes(ref nextByte, buf, buf.Length);
        }
        AddBytes(ref nextByte, buffer, length);
        return (finalBuffer, totalLen);
    }

    private static void AddBytes(ref Span<byte> nextByte, byte[] buf, int length)
    {
        buf.AsSpan(0, length).CopyTo(nextByte);
        nextByte = nextByte[length..];
        ArrayPool<byte>.Shared.Return(buf);
    }
}
