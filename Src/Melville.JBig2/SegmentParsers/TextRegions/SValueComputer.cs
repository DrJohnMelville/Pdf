namespace Melville.JBig2.SegmentParsers.TextRegions;

public abstract class SValueComputer
{
    private SValueComputer()
    {
    }

    public abstract int ComputeSValue(ref int initialS, int offset);

    private class PreIncrementImplementation : SValueComputer
    {
        public override int ComputeSValue(ref int initialS, int offset) => initialS += offset - 1;
    }

    private class PostIncrementImplementation : SValueComputer
    {
        public override int ComputeSValue(ref int initialS, int offset)
        {
            var ret = initialS;
            initialS += offset - 1;
            return ret;
        }
    }

    private static readonly SValueComputer pre = new PreIncrementImplementation();
    private static readonly SValueComputer post = new PostIncrementImplementation();
    public static SValueComputer SelectPrePost(bool shouldPreIncrement) => shouldPreIncrement ? pre : post;
}