using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.HuffmanTables;
using Melville.JBig2.SegmentParsers;

namespace Melville.JBig2.Segments;

internal enum ReferenceCorner : byte
{
    BottomLeft = 0,
    TopLeft = 1,
    BottomRight = 2,
    TopRight = 3
};

internal readonly struct TextRegionHuffmanFlags
{
    private readonly int data;

    public TextRegionHuffmanFlags(int data)
    {
        this.data = data;
    }

    /// <summary>
    /// In standard SBHUFFFFS
    /// </summary>
    public HuffmanTableSelection SbhuffFs =>
        HuffmanTableSelector.Select(data, 0, HuffmanTableSelection.B6, HuffmanTableSelection.B7);

    /// <summary>
    /// In Standard SBHUFFDS
    /// </summary>
    public HuffmanTableSelection SbhuffDs =>
        HuffmanTableSelector.Select(data, 2, HuffmanTableSelection.B8, HuffmanTableSelection.B9);

    /// <summary>
    /// In Standard SBHUFFDT
    /// </summary>
    public HuffmanTableSelection SbhuffDt =>
        HuffmanTableSelector.Select(data, 4, HuffmanTableSelection.B11, HuffmanTableSelection.B12);

    /// <summary>
    /// In Standard SBHUFFRDW
    /// </summary>
    public HuffmanTableSelection SbhuffRdw =>
        HuffmanTableSelector.Select(data, 6, HuffmanTableSelection.B14, HuffmanTableSelection.B15);

    /// <summary>
    /// In Standard SBHUFFRDH
    /// </summary>
    public HuffmanTableSelection SbhuffRdh =>
        HuffmanTableSelector.Select(data, 8, HuffmanTableSelection.B14, HuffmanTableSelection.B15);

    /// <summary>
    /// In Standard SBHUFFRDX
    /// </summary>
    public HuffmanTableSelection SbhuffRdx =>
        HuffmanTableSelector.Select(data, 10, HuffmanTableSelection.B14, HuffmanTableSelection.B15);

    /// <summary>
    /// In Standard SBHUFFRDY
    /// </summary>
    public HuffmanTableSelection SbhuffRdy =>
        HuffmanTableSelector.Select(data, 12, HuffmanTableSelection.B14, HuffmanTableSelection.B15);

    /// <summary>
    /// In standard SBHUFFRSIZE
    /// </summary>
    public HuffmanTableSelection SbHuffRSize => BitOperations.CheckBit(data, 1 << 14)
        ? HuffmanTableSelection.UserSupplied : HuffmanTableSelection.B1;
}

internal class TextRegionSegment: RegionSegment
{
    public TextRegionSegment(SegmentType type, in RegionHeader header, BinaryBitmap bitmap) : base(type, in header, bitmap)
    {
    }
}