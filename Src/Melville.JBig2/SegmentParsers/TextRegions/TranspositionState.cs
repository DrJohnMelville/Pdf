using Melville.JBig2.Segments;

namespace Melville.JBig2.SegmentParsers.TextRegions;

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
    public abstract int SIncrement(int height, int width);

    private class NotTransposedImplementation : TranspositionState
    {
        public override (int row, int col) BitmapPosition(int t, int s) => (t,s);

        protected override bool ShouldPreIncrement(ReferenceCorner corner) =>
            corner is ReferenceCorner.TopRight or ReferenceCorner.BottomRight;

        public override int SIncrement(int height, int width) => width;
    }
    
    private class TransposedImplementation: TranspositionState
    {
        public override (int row, int col) BitmapPosition(int t, int s) => (s,t);

        protected override bool ShouldPreIncrement(ReferenceCorner corner) =>
            corner is ReferenceCorner.BottomLeft or ReferenceCorner.BottomRight;
        public override int SIncrement(int height, int width) => height;
    }
}