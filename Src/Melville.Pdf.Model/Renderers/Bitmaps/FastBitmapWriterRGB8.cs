using System.Buffers;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

[StaticSingleton]
internal partial class FastBitmapWriterRGB8: IByteWriter
{
    public unsafe void WriteBytes(scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos)
    {
        while (input.Remaining >= 3 && output < nextPos)
        {
#warning -- I may be able to do this faster by reading a 4 byte uint with a extra byte and then fixing the resulting int and writing it directly to the outut buffer.
            input.TryRead(out output[2]);
            input.TryRead(out output[1]);
            input.TryRead(out *(output));
            output[3] = 0xFF;
            output += 4;
        }
    }
    
    public int MinimumInputSize => 3;
}

/*
 This is an AI chatbot item that could copy bytes a lot faster.
I think I could cast the destination span to the vectors, and save an extra store.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class PixelConverter
{
    public static byte[] ExpandRgbToRgba(ReadOnlySpan<byte> rgbSource, byte alpha = 255)
    {
        if (rgbSource.Length % 3 != 0)
            throw new ArgumentException("Source length must be a multiple of 3.");

        int pixelCount = rgbSource.Length / 3;
        int targetLength = pixelCount * 4;

        // Rent buffer from ArrayPool to avoid GC allocation spikes on large megabyte arrays
        byte[] targetArray = ArrayPool<byte>.Shared.Rent(targetLength);
        Span<byte> rgbaTarget = targetArray.AsSpan(0, targetLength);

        int srcIdx = 0;
        int dstIdx = 0;

        // Optimize using Vector128 (processes 4 pixels / 12 bytes -> 16 bytes at a time)
        if (Vector128.IsHardwareAccelerated && rgbSource.Length >= 12)
        {
            // Create a shuffle mask to map: [R1,G1,B1, R2,G2,B2, R3,G3,B3, R4,G4,B4, ...]
            // Into: [R1,G1,B1, A, R2,G2,B2, A, R3,G3,B3, A, R4,G4,B4, A]
            // -1 (or 0xFF) dictates where the Alpha byte will blend in later
            Vector128<byte> shuffleMask = Vector128.Create(
                (byte)0, 1, 2, 0xFF,    // Pixel 1
                3, 4, 5, 0xFF,          // Pixel 2
                6, 7, 8, 0xFF,          // Pixel 3
                9, 10, 11, 0xFF         // Pixel 4
            );

            // Vector containing our constant alpha channel
            Vector128<byte> alphaVector = Vector128.Create(
                (byte)0, 0, 0, alpha,
                0, 0, 0, alpha,
                0, 0, 0, alpha,
                0, 0, 0, alpha
            );

            int vectorSrcLimit = rgbSource.Length - 12;

            ref byte srcRef = ref MemoryMarshal.GetReference(rgbSource);
            ref byte dstRef = ref MemoryMarshal.GetReference(rgbaTarget);

            while (srcIdx <= vectorSrcLimit)
            {
                // Load 12 bytes of RGB (we safely read 16 bytes, ignoring the trailing 4)
                Vector128<byte> rgb = Vector128.LoadUnsafe(ref srcRef, (nuint)srcIdx);

                // Shuffle the RGB values into RGBA alignment spaces
                Vector128<byte> rgba = Vector128.Shuffle(rgb, shuffleMask);

                // Bitwise OR to blend the alpha values into the 0xFF slots
                rgba = Vector128.BitwiseOr(rgba, alphaVector);

                // Store 16 bytes of clean RGBA directly to destination memory
                rgba.StoreUnsafe(ref dstRef, (nuint)dstIdx);

                srcIdx += 12;
                dstIdx += 16;
            }
        }

        // Leftover cleanup loop (handles the last remaining < 4 pixels)
        while (srcIdx < rgbSource.Length)
        {
            rgbaTarget[dstIdx] = rgbSource[srcIdx];       // R
            rgbaTarget[dstIdx + 1] = rgbSource[srcIdx + 1]; // G
            rgbaTarget[dstIdx + 2] = rgbSource[srcIdx + 2]; // B
            rgbaTarget[dstIdx + 3] = alpha;                 // A

            srcIdx += 3;
            dstIdx += 4;
        }

        return targetArray; // Return pooled array. Remember to Return() it when finished!
    }
*/