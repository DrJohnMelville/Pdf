using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public class DirectReader : IIntegerDecoder
{
    public static IIntegerDecoder[] Instances =
    {
        FakeReaderReturnsZero.Instance, 
        new DirectReader(1),
        new DirectReader(2),
        new DirectReader(3)
    };
    private readonly int bits;
    private DirectReader(int bits) { this.bits = bits; }
    public int GetInteger(ref BitSource source) => source.ReadInt(bits);
}

public class FakeReaderReturnsZero: IIntegerDecoder
{
    public static readonly FakeReaderReturnsZero Instance = new();

    private FakeReaderReturnsZero()
    {
    }

    public int GetInteger(ref BitSource source) => 0;
}