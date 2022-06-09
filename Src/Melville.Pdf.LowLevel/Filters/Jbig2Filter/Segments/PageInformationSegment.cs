using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public readonly struct PageInformationFlags
{
    private readonly byte flags;

    public PageInformationFlags(byte flags)
    {
        this.flags = flags;
    }

    public bool Lossless => BitOperations.CheckBit(flags, 0x01);
    public bool HasRefinements => BitOperations.CheckBit(flags, 0x02);
    public bool DefaultValue => BitOperations.CheckBit(flags, 0x04);

    public CombinationOperator DefaultOperator =>
        (CombinationOperator)BitOperations.UnsignedInteger((int)flags, 3, 7);
    public bool AuxiliaryBuffers => BitOperations.CheckBit(flags, 0x20);
    public bool OverrideCombinator => BitOperations.CheckBit(flags, 0x40);
}

public readonly struct PageStripingInformation
{
    private readonly ushort rawData;

    public PageStripingInformation(ushort rawData)
    {
        this.rawData = rawData;
    }

    private const ushort StripedBitmask = 0x8000;
    public bool IsStriped => BitOperations.CheckBit(rawData, StripedBitmask);
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
