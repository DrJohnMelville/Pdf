using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Linq;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public interface IBinaryBitmap
{
    int Width { get; }
    int Height { get; }
    bool this[int row, int column] { get; set; }
}

public interface IBitmapCopyTarget : IBinaryBitmap
{
    void CopyTo(int row, int column, IBinaryBitmap source, CombinationOperator combOp);
}

public class BinaryBitmap: IBitmapCopyTarget
{
    private readonly int stride;
    public int Width { get; }
    public int Height { get; }
    private readonly byte[] bits;

    public bool this[int row, int column]
    {
        get => ComputeBitPosition(row, column).GetBit(bits);
        set => ComputeBitPosition(row, column).WriteBit(bits, value);
    }

    public void CopyTo(int row, int column, IBinaryBitmap source, CombinationOperator combOp)
    {
    }

    private BitOffset ComputeBitPosition(int row, int col) =>
        new((uint)((row * stride) + (col >> 3)), (byte)(col & 0b111));

    public BinaryBitmap(int height, int width)
    {
        Width = width;
        Height = height;
        stride = (width + 7) / 8;
        bits = new byte[stride * Height];
    }

    public Span<byte> AsByteSpan() => bits.AsSpan();

    public void FillBlack()
    {
        AsByteSpan().Fill(0xFF);
    }
}

public readonly struct BitOffset
{
    private uint ByteOffset { get;}
    private byte BitOffsetRightOfMsb {get;} // bits are numbered 0-7 MSB to LSB
    private byte BitMask {get;} // bits are numbered 0-7 MSB to LSB

    public BitOffset(uint byteOffset, byte bitOffsetRightOfMsb)
    {
        ByteOffset = byteOffset;
        BitOffsetRightOfMsb = bitOffsetRightOfMsb;
        BitMask = (byte)(1 << (7-bitOffsetRightOfMsb));
    }

    public bool GetBit(byte[] buffer) => BitMask == (buffer[ByteOffset] & BitMask);

    public void WriteBit(byte[] buffer, bool value)
    {
        if (value)
            SetBit(buffer);
        else
            ClearBit(buffer);
    }

    private void SetBit(byte[] buffer) => buffer[ByteOffset] |= BitMask;
    private void ClearBit(byte[] buffer) => buffer[ByteOffset] &= (byte)~BitMask;
}

public static class BitmapOperations
{
    public static string BitmapString(this IBinaryBitmap src) =>
        string.Join("\r\n", Enumerable.Range(0, src.Height).Select(i=>
            string.Join("", Enumerable.Range(0,src.Width).Select(j=>src[i,j]?"B":"."))
        ));

    public static BinaryBitmap AsBinaryBitmap(this string source, int height, int width )
    {
        var ret = new BinaryBitmap(height, width);
        var count = 0;
        foreach (var character in source.Where(i=> i is 'B' or '.'))
        {
            AddBit(ret, character, count++);
        }
        return ret;
    }

    private static void AddBit(BinaryBitmap ret, char character, int position)
    {
        var (row, col) = Math.DivRem(position, ret.Width);
        ret[row, col] = character == 'B';
        // this is a utility method to help with testing, so we want the exception if too much data
    }
}