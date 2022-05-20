using Melville.Hacks.Reflection;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class Segment
{
    public static readonly Segment EndOfPage = new(SegmentType.EndOfPage);
    public static readonly Segment EndOfFile = new(SegmentType.EndOfFile);
    public SegmentType Type { get; }

    protected Segment(SegmentType type)
    {
        Type = type;
    }
}