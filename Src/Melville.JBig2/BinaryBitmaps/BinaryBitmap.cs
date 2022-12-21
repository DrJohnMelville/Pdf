using System;
using System.Diagnostics;
using Melville.JBig2.Segments;

namespace Melville.JBig2.BinaryBitmaps;

[DebuggerDisplay("{this.BitmapString()}")]
internal class BinaryBitmap: IBitmapCopyTarget
{
    public int Stride { get; }
    public int Width { get; }
    public int Height { get;  private set; }
    private byte[] bits;

    public bool this[int row, int column]
    {
        get => ComputeBitPosition(row, column).GetBit(bits);
        set => ComputeBitPosition(row, column).WriteBit(bits, value);
    }
    
    public void PasteBitsFrom(int row, int column, IBinaryBitmap source, CombinationOperator combOp)
    {
        var copyRegion = new BinaryBitmapCopyRegion(
            new BinaryBitmapCopyDimension(0, source.Height, source.Height, row, Height),
            new BinaryBitmapCopyDimension(0, source.Width, source.Width, column, Width));
        
        if (copyRegion.IsTrivial()) return;
        CopyBitsFrom(source, combOp, copyRegion);
    }

    private void CopyBitsFrom(
        IBinaryBitmap source, CombinationOperator combOp, in BinaryBitmapCopyRegion copyRegion)
    {
        var srcLocation = source.ColumnLocation(copyRegion.Horizontal.SrcBegin);
        var destLocation = ColumnLocation(copyRegion.Horizontal.DestBegin);

        unsafe
        {
            fixed(byte* srcPointer = srcLocation.Array)
            fixed (byte* destPointer = destLocation.Array)
            {
                var plan = new BitCopierFactory(
                    srcLocation.Offset.BitOffsetRightOfMsb, destLocation.Offset.BitOffsetRightOfMsb,
                    copyRegion.Horizontal.Length, combOp).Create();
                
                // capture these properties 
                var rows = copyRegion.Vertical.Length;
                var sourceStride = source.Stride;
                var destStride = Stride;
                
                var currentSrc = BitBasisPointer(
                    srcPointer, copyRegion.Vertical.SrcBegin, sourceStride, srcLocation.Offset.ByteOffset);
                var currentDest = BitBasisPointer(
                    destPointer, copyRegion.Vertical.DestBegin, destStride, destLocation.Offset.ByteOffset);
                
                for (int i = 0; i < rows; i++)
                {
                    plan.Copy(currentSrc, currentDest);
                    currentSrc += sourceStride;
                    currentDest += destStride;
                }

            }
        }
            
    }

    private static unsafe byte* BitBasisPointer(byte* pointer, int row, int stride, int column) => 
        pointer + column + (row * stride);

    private BitOffset ComputeBitPosition(int row, int col)
    {
        Debug.Assert(ContainsPixel(row,col));
        return new(((row * Stride) + (col >> 3)), (byte)(col & 0b111));
    }

    public BitRowWriter StartWritingAt(int row, int column)
    {
        var pos = ComputeBitPosition(row, column);
        return new BitRowWriter(bits, pos.ByteOffset, 7 -pos.BitOffsetRightOfMsb);
    }

    public bool ContainsPixel(int row, int col) =>
        row >= 0 && col >= 0 && row < Height & col < Width;

    public (byte[] Array, BitOffset Offset) ColumnLocation(int column) => (bits, ComputeBitPosition(0, column));

    public BinaryBitmap(int height, int width)
    {
        Width = width;
        Height = height;
        Stride = (width + 7) / 8;
        bits = new byte[this.BufferLength()];
    }
    protected void ResizeToHeight(int newHeight)
    {
        Height = newHeight;
        var newBufferLength = this.BufferLength();
        if (newBufferLength > bits.Length) Array.Resize(ref bits, newBufferLength);
    }
    
    public void FillBlack() => bits.AsSpan().Fill(0xFF);

    public void CopyRow(int source, int target) =>
        bits.AsSpan(source*Stride, Stride).CopyTo(bits.AsSpan(target*Stride, Stride));

    public BitmapPointer PointerFor(int row, int col)
    {
        if (this.NoBytesLeftInRow(row, col)) return BitmapPointer.EmptyRow;
        var rowSpan = bits.AsMemory(row * Stride, Stride);
        if (col < 0) return new BitmapPointer(rowSpan, 7 - col, Width - col);
        var (colBytes, colBits) = Math.DivRem(col, 8);
        return new BitmapPointer(rowSpan[colBytes..], 7 - colBits, Width - col);
    }
}