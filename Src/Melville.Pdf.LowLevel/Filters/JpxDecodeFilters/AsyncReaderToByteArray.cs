using Melville.StreamInterfaces;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

#warning move the case where the length is known into this class
/// <summary>
/// Read an IAsyncReader into an array of bytes, as efficiently as possible.
/// </summary>
/// <param name="reader">Source of the bytes to read</param>
/// <param name="rentFinalBuffer">If true return a buffer from the ArrayPool, otherwise allocate a new byte array</param>
public readonly struct AsyncReaderToByteArray(IAsyncReader reader, bool rentFinalBuffer = true)
{
    private readonly List<byte[]> Buffers = new();

    /// <summary>
    /// Get the bytes
    /// </summary>
    /// <returns>A pair containing the byte array and an int of the number of bytes used.  If the
    /// buffer is not rented from the ArrayPool, it will be precisely the correct size.</returns>
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
