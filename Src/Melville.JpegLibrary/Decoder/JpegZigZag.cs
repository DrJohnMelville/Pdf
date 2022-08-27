using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Melville.JpegLibrary.Decoder;

public static class JpegZigZag
{
    private static ReadOnlySpan<byte> s_blockToBuffer => new byte[]
    {
        0, 1, 5, 6, 14, 15, 27, 28,
        2, 4, 7, 13, 16, 26, 29, 42,
        3, 8, 12, 17, 25, 30, 41, 43,
        9, 11, 18, 24, 31, 40, 44, 53,
        10, 19, 23, 32, 39, 45, 52, 54,
        20, 22, 33, 38, 46, 51, 55, 60,
        21, 34, 37, 47, 50, 56, 59, 61,
        35, 36, 48, 49, 57, 58, 62, 63
    };

    private static ReadOnlySpan<byte> s_bufferToBlock => new byte[]
    {
        0, 1, 8, 16, 9, 2, 3, 10,
        17, 24, 32, 25, 18, 11, 4, 5,
        12, 19, 26, 33, 40, 48, 41, 34,
        27, 20, 13, 6, 7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36,
        29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46,
        53, 60, 61, 54, 47, 55, 62, 63
    };

    /// <summary>
    /// Convert index of natural order into zig-zag order.
    /// </summary>
    /// <param name="index">Index of natural order starting from zero.</param>
    /// <returns>Index of zig-zag order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InternalBlockIndexToBuffer(int index)
    {
        Debug.Assert((uint)index < 64);
        return Unsafe.Add(ref MemoryMarshal.GetReference(s_blockToBuffer), index);
    }

    /// <summary>
    /// Convert index of zig-zag order into natural order.
    /// </summary>
    /// <param name="index">Index of zig-zag order starting from zero.</param>
    /// <returns>Index of natural order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InternalBufferIndexToBlock(int index)
    {
        Debug.Assert((uint)index < 64);
        return Unsafe.Add(ref MemoryMarshal.GetReference(s_bufferToBlock), index);
    }

    /// <summary>
    /// Convert index of natural order into zig-zag order.
    /// </summary>
    /// <param name="index">Index of natural order starting from zero.</param>
    /// <returns>Index of zig-zag order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BlockIndexToBuffer(int index)
    {
        if ((uint)index >= s_blockToBuffer.Length)
        {
            ThrowArgumentOutOfRangeException(nameof(index));
        }
        return Unsafe.Add(ref MemoryMarshal.GetReference(s_blockToBuffer), index);
    }

    /// <summary>
    /// Convert index of zig-zag order into natural order.
    /// </summary>
    /// <param name="index">Index of zig-zag order starting from zero.</param>
    /// <returns>Index of natural order.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BufferIndexToBlock(int index)
    {
        if ((uint)index >= s_bufferToBlock.Length)
        {
            ThrowArgumentOutOfRangeException(nameof(index));
        }
        return Unsafe.Add(ref MemoryMarshal.GetReference(s_bufferToBlock), index);
    }

    private static void ThrowArgumentOutOfRangeException(string parameterName)
    {
        throw new ArgumentOutOfRangeException(parameterName);
    }

}