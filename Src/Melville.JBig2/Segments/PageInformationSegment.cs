using System.Collections.Generic;
using System.IO;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;

namespace Melville.JBig2.Segments;

internal readonly struct PageInformationFlags
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

internal readonly struct PageStripingInformation
{
    private readonly int rawData;

    public PageStripingInformation(int rawData)
    {
        this.rawData = rawData;
    }

    private const int StripedBitmask = 0x8000;
    public bool IsStriped => BitOperations.CheckBit(rawData, StripedBitmask);
    public int StripeSize => rawData & (~StripedBitmask);
}

internal class PageInformationSegment : Segment
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

    public override void HandleSegment(IDictionary<uint, PageBinaryBitmap> pages, uint pageNumber) => 
        pages[pageNumber] = CreateBitmap();

    private PageBinaryBitmap CreateBitmap() => (Height, Striping.IsStriped) switch
    {
        (0xFFFF_FFFF, false) => throw new InvalidDataException("Page with unknown size must be striped"),
        (0xFFFF_FFFF, true) => new StripedBinaryBitmap(Striping.StripeSize, Width),
        (_) => new PageBinaryBitmap(Height, Width)
    };
}
