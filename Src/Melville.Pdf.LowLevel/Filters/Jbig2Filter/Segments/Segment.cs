using Melville.Hacks.Reflection;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class Segment
{
    public static readonly Segment EndOfPage = new(SegmentType.EndOfPage, uint.MaxValue);
    public static readonly Segment EndOfFile = new(SegmentType.EndOfFile, uint.MaxValue);
    public SegmentType Type { get; }
    public uint Number { get; }

    protected Segment(SegmentType type, uint number)
    {
        Type = type;
        Number = number;
    }
}