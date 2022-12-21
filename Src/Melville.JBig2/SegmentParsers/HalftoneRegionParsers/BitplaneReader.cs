using System.Buffers;
using Melville.INPC;
using Melville.JBig2.ArithmeticEncodings;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.Segments;

namespace Melville.JBig2.SegmentParsers.HalftoneRegionParsers;

internal abstract class Bitplane: BinaryBitmap
{
    protected Bitplane(int height, int width) : base(height, width)
    {
    }

    public abstract void ReadFrom(ref SequenceReader<byte> source);
}

internal class MmrBitplane : Bitplane
{
    public MmrBitplane(int height, int width) : base(height, width)
    {
    }

    public override void ReadFrom(ref SequenceReader<byte> source)
    {
        this.ReadMmrEncodedBitmap(ref source, true);
    }
}

internal abstract class ArithmeticBitplane : Bitplane, ISkipBitmap
{
    private readonly ArithmeticGenericRegionDecodeProcedure reader;
    public ArithmeticBitplane(int height, int width, GenericRegionTemplate template) : base(height, width)
    {
        reader = new ArithmeticGenericRegionDecodeProcedure(this, new MQDecoder(),
            new ArithmeticBitmapReaderContext(ComputeTemplate(template)), 0, this);
    }

    public override void ReadFrom(ref SequenceReader<byte> source) => reader.Read(ref source);

    private BitmapTemplate ComputeTemplate(GenericRegionTemplate template)
    {
        var fact = new BitmapTemplateFactory(template);
        fact.AddPoint(-1, FirstXTemplateLocation(template));
        if (fact.ExpectedAdaptivePixels() > 1)
        {
            fact.AddPoint(-1,-3);
            fact.AddPoint(-2, 2);
            fact.AddPoint(-2,-2);
        }
        
        return fact.Create();
    }

    private static sbyte FirstXTemplateLocation(GenericRegionTemplate template) => 
        (sbyte)(template is GenericRegionTemplate.GB0 or GenericRegionTemplate.GB1?3:2);
    
    public abstract bool ShouldSkipPixel(int row, int column);
}

internal sealed class NonskippingBitplane : ArithmeticBitplane
{
    public NonskippingBitplane(int height, int width, GenericRegionTemplate template) : base(height, width, template)
    {
    }

    public override bool ShouldSkipPixel(int row, int column) => false;
}

internal sealed partial class SkippingBitplane : ArithmeticBitplane
{
    [FromConstructor] private readonly int hgx;
    [FromConstructor] private readonly int hgy;
    [FromConstructor] private readonly int hrx;
    [FromConstructor] private readonly int hry;
    [FromConstructor] private readonly int patternWidth;
    [FromConstructor] private readonly int patternHeight;
    [FromConstructor] private readonly int targetWidth;
    [FromConstructor] private readonly int targetHeight;

    public override bool ShouldSkipPixel(int row, int column)
    {
        return ShouldSkipDimenstion(X(row, column), patternWidth, targetWidth) ||
               ShouldSkipDimenstion(Y(row, column), patternHeight, targetHeight);
    }

    private int X(int row, int column) => 
        GridVectorToBitmapPosition(hgx + (row * hry) + (column * hrx));
    private int Y(int row, int column) => 
        GridVectorToBitmapPosition(hgy + (row * hrx) - (column * hry));

    private static int GridVectorToBitmapPosition(int x) => x >> 8;

    private bool ShouldSkipDimenstion(int position, int patternSize, int targetSize) => 
        (position + patternSize <= 0) || position >= targetSize;
}