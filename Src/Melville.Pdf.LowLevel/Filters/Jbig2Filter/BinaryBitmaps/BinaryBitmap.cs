using System;
using System.Diagnostics;
using System.IO;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public interface IBinaryBitmap
{
    int Width { get; }
    int Height { get;}
    bool this[int row, int column] {  get; set; }
    int Stride { get; }
    (byte[] Array, BitOffset Offset) ColumnLocation (int column);
    bool ContainsPixel(int row, int col);
    BitmapPointer PointerFor(int row, int col);
}

public interface IBitmapCopyTarget : IBinaryBitmap
{
    void PasteBitsFrom(int row, int column, IBinaryBitmap source, CombinationOperator combOp);
}

[DebuggerDisplay("{this.BitmapString()}")]
public class BinaryBitmap: IBitmapCopyTarget
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
        
        if (copyRegion.UseSlowAlgorithm)
            PasteBitsFromSlow(source, combOp, copyRegion);
        else
            PasteBitsFromFast(source, combOp, copyRegion);
    }

    private void PasteBitsFromFast(
        IBinaryBitmap source, CombinationOperator combOp, in BinaryBitmapCopyRegion copyRegion)
    {
        var srcLocation = source.ColumnLocation(copyRegion.Horizontal.SrcBegin);
        var destLocation = ColumnLocation(copyRegion.Horizontal.DestBegin);
        unsafe
        {
            fixed(byte* srcPointer = srcLocation.Array)
            fixed (byte* destPointer = destLocation.Array)
            {
                var plan = BitCopierFactory.Create(
                    srcLocation.Offset.BitOffsetRightOfMsb, destLocation.Offset.BitOffsetRightOfMsb,
                    (uint)copyRegion.Horizontal.Length, combOp);
                
                // capture these properties 
                var rows = copyRegion.Vertical.Length;
                var sourceStride = source.Stride;
                var destStride = Stride;
                
                var currentSrc = BitBasisPointer(
                    srcPointer, copyRegion.Vertical.SrcBegin, sourceStride, (uint)srcLocation.Offset.ByteOffset);
                var currentDest = BitBasisPointer(
                    destPointer, copyRegion.Vertical.DestBegin, destStride, (uint)destLocation.Offset.ByteOffset);
                
                for (int i = 0; i < rows; i++)
                {
                    plan.Copy(currentSrc, currentDest);
                    currentSrc += sourceStride;
                    currentDest += destStride;
                }

            }
        }
            
    }

    private static unsafe byte* BitBasisPointer(byte* pointer, int row, int stride, uint column) => 
        pointer + column + (row * stride);

    private void PasteBitsFromSlow(
        IBinaryBitmap source, CombinationOperator combOp, in BinaryBitmapCopyRegion copyRegion)
    {
        var destRow = copyRegion.Vertical.DestBegin;
        for (var i = copyRegion.Vertical.SrcBegin; i < copyRegion.Vertical.SrcExclusiveEnd; i++, destRow++)
        {
            int destColumn = copyRegion.Horizontal.DestBegin;
            for (int j = copyRegion.Horizontal.SrcBegin; j < copyRegion.Horizontal.SrcExclusiveEnd; j++, destColumn++)
            {
                AssignPixel(destRow, destColumn, source[i, j], combOp);
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

    private BitOffset ComputeBitPosition(int row, int col)
    {
        Debug.Assert(ContainsPixel(row,col));
        return new(((row * Stride) + (col >> 3)), (byte)(col & 0b111));
    }

    public bool ContainsPixel(int row, int col) =>
        row >= 0 && col >= 0 && row < Height & col < Width;

    public (byte[] Array, BitOffset Offset) ColumnLocation(int column) => (bits, ComputeBitPosition(0, column));

    public BinaryBitmap(int height, int width)
    {
        Width = width;
        Height = height;
        Stride = (width + 7) / 8;
        bits = new byte[BufferLength()];
    }

    private int BufferLength() => Stride * Height;

    protected void ResizeToHeight(int newHeight)
    {
        Height = newHeight;
        var newBufferLength = BufferLength();
        if (newBufferLength > bits.Length) Array.Resize(ref bits, newBufferLength);
    }
    
    public void FillBlack() => bits.AsSpan().Fill(0xFF);

    //I think I am going to eventually need to genenralize this to arbitrary bitmaps.
    //I ought to be able to use the bitmap copy infractructure to copy from myself
    public void CopyRow(int source, int target) =>
        bits.AsSpan(source*Stride, Stride).CopyTo(bits.AsSpan(target*Stride, Stride));

    public void InvertBits()
    {
        for (int i = 0; i < bits.Length; i++)
        {
            bits[i] = (byte)~bits[i];
        }
    }
    
    public Stream BitsAsStream() => new MemoryStream(bits, 0, BufferLength());

    public BitmapPointer PointerFor(int row, int col)
    {
        if (this.NoBytesLeftInRow(row, col)) return BitmapPointer.EmptyRow;
        var rowSpan = bits.AsMemory(row * Stride, Stride);
        if (col < 0) return new BitmapPointer(rowSpan, (ushort)(7 - col), (ushort)(Width - col));
        var (colBytes, colBits) = Math.DivRem(col, 8);
        return new BitmapPointer(rowSpan[colBytes..], (ushort)(7 - colBits), (ushort)(Width - col));
    }
}

public static class BinaryBitmapOperations
{
    public static bool NoBytesLeftInRow(this IBinaryBitmap bitmap, int row, int col) => 
        row < 0 || row >= bitmap.Height || col >= bitmap.Width;
    
}