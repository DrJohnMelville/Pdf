using System;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

[Flags]
public enum PageInformationFlags : byte
{
    Lossless             = 0b00000001,
    HasRefinements       = 0b00000010,
    DefaultValue         = 0b00000100,
    AuxiliaryBuffers     = 0b00100000,
    OverrideCombinator = 0b01000000,
}

public static class PageInformationFlagOperation
{
    public static CombinationOperator DefaultOperator(this PageInformationFlags flags) =>
        (CombinationOperator)(0b11 & ((int)flags >> 3));
}

public readonly struct PageStripingInformation
{
    private readonly ushort rawData;

    public PageStripingInformation(ushort rawData)
    {
        this.rawData = rawData;
    }

    private const ushort StripedBitmask = 0x8000;
    public bool IsStriped => (rawData & StripedBitmask) == StripedBitmask;
    public int StripeSize => rawData & (~StripedBitmask);
}

public class PageInformationSegment : Segment
{
    public uint Width { get; }
    public uint Height { get; }
    public uint XResolution { get; }
    public uint YResolution { get; }
    public PageInformationFlags Flags { get; }
    public PageStripingInformation Striping { get; }

    public PageInformationSegment(uint width, uint height, uint xResolution, uint yResolution, PageInformationFlags flags, PageStripingInformation striping) : 
        base(SegmentType.PageInformation)
    {
        Width = width;
        Height = height;
        XResolution = xResolution;
        YResolution = yResolution;
        Flags = flags;
        Striping = striping;
    }
}
