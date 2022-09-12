using Melville.JBig2.Segments;

namespace Melville.JBig2.SegmentDictionaries;

public interface ISegmentDictionary
{
    Segment this[uint segmentNumber] { get; set; }
}
public class SegmentDictionary: ISegmentDictionary
{
    private Dictionary<uint, Segment> item = new();

    public Segment this[uint segmentNumber]
    {
        get => item[segmentNumber];
        set => item[segmentNumber] = value;
    }
}