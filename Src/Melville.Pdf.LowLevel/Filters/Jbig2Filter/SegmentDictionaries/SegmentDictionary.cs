using System.Collections.Generic;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentDictionaries;

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