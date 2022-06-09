namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public readonly struct BinaryBitmapCopyRegion
{
    public int DestinationFirstRow { get; }
    public int SourceFirstRow { get; }
    public int SourceExclusiveEndRow { get; }
    public int DestinationFirstCol { get; }
    public int SourceFirstCol { get; }
    public int SourceExclusiveEndCol { get; }

    public BinaryBitmapCopyRegion(int row, int column, IBinaryBitmap source, IBinaryBitmap destination)
    {
        #warning need to adjust for IBinaryBitmaps with regions that do not exist
        (DestinationFirstRow, SourceFirstRow, SourceExclusiveEndRow) = 
            ComputeOverlap(0, source.Height, row, destination.Height);
        (DestinationFirstCol, SourceFirstCol, SourceExclusiveEndCol) = 
            ComputeOverlap(0, source.Width, column, destination.Width);

    }   
    
    private static (int FirstInDestination, int FirstInSource, int ExclusiveEndInSource) 
        ComputeOverlap(int sourceStart, int sourceEnd, int destStart, int destEnd)
    {
        var offset = destStart - sourceStart;
        var length = sourceEnd - sourceStart;
        var candidateEnd = destStart + length;
        if (candidateEnd > destEnd) length -= (candidateEnd - destEnd);
        var firstInSource = offset < 0 ? -offset : 0;
        return (offset+firstInSource, firstInSource, length);
    }

    public bool UseSlowAlgorithm => RowLength < 9;
    public uint RowLength => (uint)(SourceExclusiveEndCol - SourceFirstCol);
    public uint Height => (uint)(SourceExclusiveEndRow - SourceFirstRow);
}