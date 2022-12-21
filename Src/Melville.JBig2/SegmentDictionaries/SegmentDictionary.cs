using System.Collections.Generic;
using Melville.JBig2.Segments;

namespace Melville.JBig2.SegmentDictionaries;

internal interface ISegmentDictionary
{
    Segment this[uint segmentNumber] { get; set; }
}
internal class SegmentDictionary: ISegmentDictionary
{
    private Dictionary<uint, Segment> item = new();

    public Segment this[uint segmentNumber]
    {
        get => item[segmentNumber];
        set => item[segmentNumber] = value;
    }
}