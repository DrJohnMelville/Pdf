using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    void PasteBitsFrom(int row, int column, IBinaryBitmap source, CombinationOperator combOp);
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

    public void PasteBitsFrom(int row, int column, IBinaryBitmap source, CombinationOperator combOp)
    {
        #warning -- this is ripe with opportunities to optimize
        for (int i = 0; i < source.Height; i++)
        {
            var outputRow = row + i;
            if (!IsValid(outputRow, Height)) continue;
            for (int j = 0; j < source.Width; j++)
            {
                var outputCol = column + j;
                if (!IsValid(outputCol, Width)) continue;
                AssignPixel(outputRow, outputCol, source[i, j], combOp);
            }
        }
    }

    private void AssignPixel(int outputRow, int outputCol, bool sourcePixel, CombinationOperator combinationOperator)
    {
        switch (combinationOperator)
        {
            case CombinationOperator.Or:    
                this[outputRow, outputCol] |= sourcePixel;                
                break;
            case CombinationOperator.And:
                this[outputRow, outputCol] &= sourcePixel;                
                break;
            case CombinationOperator.Xor:
                this[outputRow, outputCol] ^= sourcePixel;                
                break;
            case CombinationOperator.Xnor:
                this[outputRow, outputCol] = !(this[outputRow, outputCol] ^ sourcePixel);                
                break;
            case CombinationOperator.Replace:
                this[outputRow, outputCol] = sourcePixel;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(combinationOperator), combinationOperator, null);
        }
    }

    private bool IsValid(int i, int size) => i >= 0 && i < size;

    private BitOffset ComputeBitPosition(int row, int col)
    {
        Debug.Assert(row >= 0);
        Debug.Assert(row < Height);
        Debug.Assert(col >= 0);
        Debug.Assert(col<= Width);
        return new((uint)((row * stride) + (col >> 3)), (byte)(col & 0b111));
    }

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