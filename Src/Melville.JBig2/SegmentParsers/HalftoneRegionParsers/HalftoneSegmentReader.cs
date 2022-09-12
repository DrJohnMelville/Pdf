using System.Buffers;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.Segments;

namespace Melville.JBig2.SegmentParsers.HalftoneRegionParsers;

public ref struct HalftoneSegmentReader
{
    public RegionHeader Header { get; }
    private readonly HalftoneRegionFlags regionFlags;
    private readonly int grayScaleWidth; // HGW
    private readonly int grayScaleHeight; // HGW
    private readonly int grayScaleXOffset; // HGX
    private readonly int grayScaleYOffset; // HGY
    private readonly int vectorX; // HRY
    private readonly int vectorY; // HRY
    private readonly DictionarySegment dictionary;

    // order of arguments is correctness critial because HalftoneSegmentParser uses this order to
    // read data out of the bitstream
    public HalftoneSegmentReader(
        RegionHeader regionHeader, HalftoneRegionFlags regionFlags, uint grayScaleWidth, uint grayScaleHeight, 
        int grayScaleXOffset, int grayScaleYOffset, uint vectorX, uint vectorY, DictionarySegment dictionary)
    {
        this.Header = regionHeader;
        this.regionFlags = regionFlags;
        this.grayScaleWidth = (int)grayScaleWidth;
        this.grayScaleHeight = (int)grayScaleHeight;
        this.grayScaleXOffset = grayScaleXOffset;
        this.grayScaleYOffset = grayScaleYOffset;
        this.vectorX = (int)vectorX;
        this.vectorY = (int)vectorY;
        this.dictionary = dictionary;
    }

    public void ReadBitmap(ref SequenceReader<byte> reader, BinaryBitmap targetBitmap)
    {
        FillWithBackground(targetBitmap);

        var spanLength = grayScaleHeight * grayScaleWidth;
        var greyScaleBitmap = new int[spanLength];
        var gsb = ReadGrayScaleBitmap(ref reader, greyScaleBitmap);

        WriteHalftone(targetBitmap, gsb);
    }

    private void WriteHalftone(BinaryBitmap targetBitmap, GrayScaleBitmap gsb)
    {
        var xRowValue = grayScaleXOffset;
        var yRowValue = grayScaleYOffset;
        for (int m = 0; m < grayScaleHeight; m++)
        {
            var xColValue = xRowValue;
            var yColValue = yRowValue;
            for (int n = 0; n < grayScaleWidth; n++)
            {
                targetBitmap.PasteBitsFrom(yColValue >> 8, xColValue >> 8,
                    dictionary.ExportedSymbols.Span[gsb[m, n]], regionFlags.CombinationOperator);
                xColValue += vectorX;
                yColValue -= vectorY;
            }
            xRowValue += vectorY;
            yRowValue += vectorX;
        }
    }

    private GrayScaleBitmap ReadGrayScaleBitmap(ref SequenceReader<byte> reader, Span<int> data)
    {
        var gsb = new GrayScaleBitmap(data, grayScaleWidth);
        var bitPlane = regionFlags.UseMMR ?
            CreateMmrBitplane():
            CreateArithmeticBitplane();
        
        var bitsPerValue = IntLog.CeilingLog2Of((uint)dictionary.ExportedSymbols.Length);
        var bitValue = 1 << (bitsPerValue - 1);
        
        bitPlane.ReadFrom(ref reader);
        gsb.CopyBinaryBitmap(bitPlane, bitValue);
  
        var priorBitValue = bitValue;
        bitValue >>= 1;
        
        for (; bitValue > 0; bitValue >>= 1, priorBitValue >>= 1)
        {
            bitPlane.ReadFrom(ref reader);
            gsb.ProcessBinaryBitmap(bitPlane, bitValue, priorBitValue);
        }

        return gsb;
    }

    private Bitplane CreateMmrBitplane()
    {
        return new MmrBitplane(grayScaleHeight, grayScaleWidth);
    }

    private Bitplane CreateArithmeticBitplane()
    {
        return regionFlags.EnableSkip ?
            new SkippingBitplane(grayScaleHeight, grayScaleWidth, regionFlags.Template,
                grayScaleXOffset, grayScaleYOffset, vectorX, vectorY, 
                dictionary.ExportedSymbols.Span[0].Width, 
                dictionary.ExportedSymbols.Span[0].Height,
                (int)Header.Width, (int)Header.Height):
            new NonskippingBitplane(grayScaleHeight, grayScaleWidth, regionFlags.Template);
    }

    private void FillWithBackground(BinaryBitmap targetBitmap)
    {
        if (regionFlags.DefaultPixel) targetBitmap.FillBlack();
    }
}