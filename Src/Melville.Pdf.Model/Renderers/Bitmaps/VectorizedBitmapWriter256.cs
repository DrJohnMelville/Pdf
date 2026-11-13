using Melville.INPC;
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal static class VectorizedBitmapWriter512
{
    public const int SourceBytes = 48;
    public const int DestBytes = 64;

    private static readonly Vector512<byte> shuffle = Vector512.Create(
        2, 1, 0, 0xFF,
        5, 4, 3, 0xFF,
        8, 7, 6, 0xFF,
        11, 10, 9, 0xFF,
        14, 13, 12, 0xFF,
        17, 16, 15, 0xFF,
        20, 19, 18, 0xFF,
        23, 22, 21, 0xFF,
        26, 25, 24, 0xFF,
        29, 28, 27, 0xFF,
        32, 21, 30, 0xFF,
        35, 34, 33, 0xFF,
        38, 37, 36, 0xFF,
        41, 40, 39, 0xFF,
        44, 43, 42, 0xFF,
        47, 46, 45, 0xFF
        );
    
    private static readonly Vector512<byte> lastByte = Vector512.Create(
        0,0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF
        );

    public static void WriteBytes(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        var outputSpan = MemoryMarshal.Cast<byte, Vector512<byte>>(output);
        int i;
        var sourceSpan = input.Peek();
        ref var source = ref MemoryMarshal.GetReference(sourceSpan);
        for (i = 0; ((SourceBytes*(i+1)) <= sourceSpan.Length) &&
                        i < outputSpan.Length; i++)
        {
            var data = Vector512.LoadUnsafe(ref source, (nuint) i * SourceBytes);
            data = Vector512.Shuffle(data, shuffle);
            outputSpan[i] = Vector512.BitwiseOr(data, lastByte);
        }
        input.Advance(i*SourceBytes);
        output = output[(i*DestBytes)..];
    }
}

internal static class VectorizedBitmapWriter256
{
    public const int SourceBytes = 24;
    public const int DestBytes = 32;

    private static readonly Vector256<byte> shuffle = Vector256.Create(
        2, 1, 0, 0xFF,
        5, 4, 3, 0xFF,
        8, 7, 6, 0xFF,
        11, 10, 9, 0xFF,
        14, 13, 12, 0xFF,
        17, 16, 15, 0xFF,
        20, 19, 18, 0xFF,
        23, 22, 21, 0xFF
        );

    private static readonly Vector256<byte> lastByte = Vector256.Create(
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF
        );

    public static void WriteBytes(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        var outputSpan = MemoryMarshal.Cast<byte, Vector256<byte>>(output);
        int i;
        var sourceSpan = input.Peek();
        ref var source = ref MemoryMarshal.GetReference(sourceSpan);
        for (i = 0; ((SourceBytes * (i + 1)) <= sourceSpan.Length) &&
                        i < outputSpan.Length; i++)
        {
            var data = Vector256.LoadUnsafe(ref source, (nuint)i * SourceBytes);
            data = Vector256.Shuffle(data, shuffle);
            outputSpan[i] = Vector256.BitwiseOr(data, lastByte);
        }
        input.Advance(i * SourceBytes);
        output = output[(i * DestBytes)..];
    }
}

internal static class VectorizedBitmapWriter128
{
    public const int SourceBytes = 12;
    public const int DestBytes = 16;

    private static readonly Vector128<byte> shuffle = Vector128.Create(
        2, 1, 0, 0xFF,
        5, 4, 3, 0xFF,
        8, 7, 6, 0xFF,
        11, 10, 9, 0xFF
        );

    private static readonly Vector128<byte> lastByte = Vector128.Create(
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF
        );

    public static void WriteBytes(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        var outputSpan = MemoryMarshal.Cast<byte, Vector128<byte>>(output);
        int i;
        var sourceSpan = input.Peek();
        ref var source = ref MemoryMarshal.GetReference(sourceSpan);
        for (i = 0; ((SourceBytes * (i + 1)) <= sourceSpan.Length) &&
                        i < outputSpan.Length; i++)
        {
            var data = Vector128.LoadUnsafe(ref source, (nuint)i * SourceBytes);
            data = Vector128.Shuffle(data, shuffle);
            outputSpan[i] = Vector128.BitwiseOr(data, lastByte);
        }
        input.Advance(i * SourceBytes);
        output = output[(i * DestBytes)..];
    }
}
internal static class VectorizedBitmapWriter64
{
    public const int SourceBytes = 6;
    public const int DestBytes = 8;

    private static readonly Vector64<byte> shuffle = Vector64.Create(
        2, 1, 0, 0xFF,
        5, 4, 3, 0xFF
        );

    private static readonly Vector64<byte> lastByte = Vector64.Create(
        0, 0, 0, 0xFF,
        0, 0, 0, 0xFF
        );

    public static void WriteBytes(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        var outputSpan = MemoryMarshal.Cast<byte, Vector64<byte>>(output);
        int i;
        var sourceSpan = input.Peek();
        ref var source = ref MemoryMarshal.GetReference(sourceSpan);
        for (i = 0; ((SourceBytes * (i + 1)) <= sourceSpan.Length) &&
                        i < outputSpan.Length; i++)
        {
            var data = Vector64.LoadUnsafe(ref source, (nuint)i * SourceBytes);
            data = Vector64.Shuffle(data, shuffle);
            outputSpan[i] = Vector64.BitwiseOr(data, lastByte);
        }
        input.Advance(i * SourceBytes);
        output = output[(i * DestBytes)..];
    }
}

