using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Adapters;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Melville.Parsing.ReaderExtensions;

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
    public (byte[], int) Read()
    {
        while (true)
        {
            var buffer = AllocateInitialBuffer();
            var length = reader.AsISyncReader().ReadAtLeast(buffer, buffer.Length, false);
            if (length < buffer.Length) return MakeFinalBuffer(buffer, length);
            Buffers.Add(buffer);
        }
    }

    /// <summary>
    /// Get the bytes
    /// </summary>
    /// <returns>A pair containing the byte array and an int of the number of bytes used.  If the
    /// buffer is not rented from the ArrayPool, it will be precisely the correct size.</returns>
    public async ValueTask<(byte[], int)> ReadAsync()
    {
        while (true)
        {
            var buffer = AllocateInitialBuffer();
            var length = await reader.ReadAtLeastAsync(buffer, buffer.Length, default, false).CA();
            if (length < buffer.Length) return MakeFinalBuffer(buffer, length);
            Buffers.Add(buffer);
        }
    }

    private byte[] AllocateInitialBuffer()
    {
        if (Buffers.Count is 0 && reader is IStreamLength { Length: > 0 and var length })
            return AllocateFinalBuffer((int)length);
        return ArrayPool<byte>.Shared.Rent(4096);

    }

    (byte[], int) MakeFinalBuffer(byte[] buffer, int length)
    {
        if (Buffers.Count is 0 && rentFinalBuffer)
            return (buffer, length);
        if (Buffers.Count is 1 && length is 0) // this happens a lot when we set the first buffer to the declared length.
        {
            TryReturnBuffer(buffer);
            return (Buffers[0], Buffers[0].Length);
        }
        var totalLen = length + Buffers.Sum(i => i.Length);
        byte[] finalBuffer = AllocateFinalBuffer(totalLen);
        var nextByte = finalBuffer.AsSpan();
        foreach (var buf in Buffers)
        {
            AddBytes(ref nextByte, buf, buf.Length);
        }
        AddBytes(ref nextByte, buffer, length);
        return (finalBuffer, totalLen);
    }



    private byte[] AllocateFinalBuffer(int totalLen)
    {
        return rentFinalBuffer ?
            ArrayPool<byte>.Shared.Rent(totalLen) :
                                new byte[totalLen];
    }

    private static void AddBytes(ref Span<byte> nextByte, byte[] buf, int length)
    {
        buf.AsSpan(0, length).CopyTo(nextByte);
        nextByte = nextByte[length..];
        TryReturnBuffer(buf);
    }

    private static void TryReturnBuffer(byte[] buf)
    {
        if (BitOperations.IsPow2(buf.Length)) ArrayPool<byte>.Shared.Return(buf);
    }
}