using System;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public readonly struct BinaryBitmapWriter
{
    private readonly IBitmapCopyTarget target;
    private readonly TranspositionState transposition;
    private readonly SValueComputer sValueComputer;
    private readonly ReferenceCorner referenceCorner;
    private readonly CombinationOperator operation;

    public BinaryBitmapWriter(IBitmapCopyTarget target, bool transposed, ReferenceCorner referenceCorner, CombinationOperator operation)
    {
        this.target = target;
        this.referenceCorner = referenceCorner;
        this.operation = operation;
        transposition = transposed ? TranspositionState.Transposed : TranspositionState.NotTransposed;
        sValueComputer = transposition.SelectSValueComputer(referenceCorner);
    }

    public void WriteBitmap(int t, ref int s, IBinaryBitmap source)
    {
        var (row, col) = AdjustForCorner(FinalBitmapPosition(t, ref s, source), source);
        target.PasteBitsFrom(row,col, source, operation);
    }

    private (int row, int col) FinalBitmapPosition(int t, ref int s, IBinaryBitmap source) => 
        transposition.BitmapPosition(t, ComputeNewSValue(ref s, source));

    private int ComputeNewSValue(ref int s, IBinaryBitmap source) => 
        sValueComputer.ComputeSValue(ref s, transposition.SIncrement(source));

    private (int row, int col) AdjustForCorner((int row, int col) position, IBinaryBitmap source) =>
        referenceCorner switch
        {
            ReferenceCorner.BottomLeft => (1+position.row - source.Height, position.col),
            ReferenceCorner.TopLeft => position,
            ReferenceCorner.BottomRight => (1+position.row - source.Height, 1+position.col-source.Width),
            ReferenceCorner.TopRight => (position.row, 1+position.col - source.Width),
            _ => throw new ArgumentOutOfRangeException()
        };
}