using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public abstract class TranspositionState
{
    public static readonly TranspositionState NotTransposed = new NotTransposedImplementation();
    public static readonly TranspositionState Transposed = new TransposedImplementation();
    private TranspositionState()
    {
    }

    public abstract (int row, int col) BitmapPosition(int t, int s);
    protected abstract bool ShouldPreIncrement(ReferenceCorner corner);
    public SValueComputer SelectSValueComputer(ReferenceCorner corner) =>
        SValueComputer.SelectPrePost(ShouldPreIncrement(corner));
    public abstract int SIncrement(IBinaryBitmap source);

    private class NotTransposedImplementation : TranspositionState
    {
        public override (int row, int col) BitmapPosition(int t, int s) => (t,s);

        protected override bool ShouldPreIncrement(ReferenceCorner corner) =>
            corner is ReferenceCorner.TopRight or ReferenceCorner.BottomRight;

        public override int SIncrement(IBinaryBitmap source) => source.Width;
    }
    
    private class TransposedImplementation: TranspositionState
    {
        public override (int row, int col) BitmapPosition(int t, int s) => (s,t);

        protected override bool ShouldPreIncrement(ReferenceCorner corner) =>
            corner is ReferenceCorner.BottomLeft or ReferenceCorner.BottomRight;
        public override int SIncrement(IBinaryBitmap source) => source.Height;
    }
}