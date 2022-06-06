using System.IO;
using System.Threading;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public enum ReferenceCorner : byte
{
    BottomLeft = 0,
    TopLeft = 1,
    BottomRight = 2,
    TopRight = 3
};

public readonly struct TextRegionHuffmanFlags
{
    private readonly ushort data;

    public TextRegionHuffmanFlags(ushort data)
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

public class TextRegionSegment: RegionSegment
{
    public TextRegionSegment(SegmentType type, in RegionHeader header, BinaryBitmap bitmap) : base(type, in header, bitmap)
    {
    }
}