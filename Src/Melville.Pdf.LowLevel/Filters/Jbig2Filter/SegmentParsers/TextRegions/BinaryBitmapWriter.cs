using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;
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
        var (row, col) = AdjustForCorner(FinalBitmapPosition(t, ref s, source.Height, source.Width),
            source.Height, source.Width);
        target.PasteBitsFrom(row,col, source, operation);
    }

    private (int row, int col) FinalBitmapPosition(int t, ref int s, int height, int width) => 
        transposition.BitmapPosition(t, ComputeNewSValue(ref s, height, width));

    private int ComputeNewSValue(ref int s, int height, int width) => 
        sValueComputer.ComputeSValue(ref s, transposition.SIncrement(height, width));

    private (int row, int col) AdjustForCorner((int row, int col) position, int height, int width) =>
        referenceCorner switch
        {
            ReferenceCorner.BottomLeft => (1+position.row - height, position.col),
            ReferenceCorner.TopLeft => position,
            ReferenceCorner.BottomRight => (1+position.row - height, 1+position.col-width),
            ReferenceCorner.TopRight => (position.row, 1+position.col - width),
            _ => throw new ArgumentOutOfRangeException()
        };

    public void RefineBitsFrom(int t, ref int s, IBinaryBitmap referenceBitmap,
        int refY, int refX, int refHeight, int refWidth, IEncodedReader reader,
        RefinementTemplateSet refinementTemplateSet, ref SequenceReader<byte> source)
    {
        var (row, col) = AdjustForCorner(FinalBitmapPosition(t, ref s, refHeight, refWidth), refHeight, refWidth);
        
        reader.InvokeSymbolRefinement(OffsetBitmapFactory.Create(target, row, col, refHeight, refWidth),
            OffsetBitmapFactory.Create(referenceBitmap, -refY, -refX), 0, refinementTemplateSet, ref source);
    }
}