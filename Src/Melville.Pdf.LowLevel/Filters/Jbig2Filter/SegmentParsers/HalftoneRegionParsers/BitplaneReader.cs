using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.HalftoneRegionParsers;

public abstract class Bitplane: BinaryBitmap
{
    protected Bitplane(int height, int width) : base(height, width)
    {
    }

    public abstract void ReadFrom(ref SequenceReader<byte> source);
}

public class MmrBitplane : Bitplane
{
    public MmrBitplane(int height, int width) : base(height, width)
    {
    }

    public override void ReadFrom(ref SequenceReader<byte> source)
    {
        this.ReadMmrEncodedBitmap(ref source, true);
    }
}

public class ArithmeticBitplane : Bitplane
{
    private readonly AritmeticBitmapReader reader;
    public ArithmeticBitplane(int height, int width, GenericRegionTemplate template, bool skip) : base(height, width)
    {
        reader = new AritmeticBitmapReader(this, new MQDecoder(),
            new ArithmeticBitmapReaderContext(ComputeTemplate(template)), 0, skip);
    }

    public override void ReadFrom(ref SequenceReader<byte> source) => reader.Read(ref source);

    private BitmapTemplate ComputeTemplate(GenericRegionTemplate template)
    {
        var fact = new BitmapTemplateFactory(template);
        fact.AddPoint(-1, FirstXTemplateLocation(template));
        if (fact.ExpectedAdaptivePixels() > 1)
        {
            fact.AddPoint(-1,-3);
            fact.AddPoint(-2,-2);
        }

        return fact.Create();
    }

    private static sbyte FirstXTemplateLocation(GenericRegionTemplate template) => 
        (sbyte)(template is GenericRegionTemplate.GB0 or GenericRegionTemplate.GB1?3:2);
}