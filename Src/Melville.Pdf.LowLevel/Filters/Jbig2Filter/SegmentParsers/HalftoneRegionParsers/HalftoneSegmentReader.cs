using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.GenericRegionParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.HalftoneRegionParsers;

public ref struct HalftoneSegmentReader
{
    private readonly RegionHeader regionHeader;
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
        this.regionHeader = regionHeader;
        this.regionFlags = regionFlags;
        this.grayScaleWidth = (int)grayScaleWidth;
        this.grayScaleHeight = (int)grayScaleHeight;
        this.grayScaleXOffset = grayScaleXOffset;
        this.grayScaleYOffset = grayScaleYOffset;
        this.vectorX = (int)vectorX;
        this.vectorY = (int)vectorY;
        this.dictionary = dictionary;
    }

    public unsafe HalftoneSegment ReadSegment(ref SequenceReader<byte> reader)
    {
        var targetBitmap = regionHeader.CreateTargetBitmap();
        FillWithBackground(targetBitmap);

        var spanLength = grayScaleHeight * grayScaleWidth;
        var greyScaleBitmapPointer = stackalloc int[spanLength];
        var grayScaleBitmapSpan = new Span<int>(greyScaleBitmapPointer, spanLength);
        var gsb = ReadGrayScaleBitmap(ref reader, grayScaleBitmapSpan);

        WriteHalftone(targetBitmap, gsb);

        return new HalftoneSegment(SegmentType.ImmediateLosslessHalftoneRegion, regionHeader,
            targetBitmap);
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
                targetBitmap.PasteBitsFrom(yColValue >> 8, xColValue >> 8, dictionary.ExportedSymbols.Span[gsb[m, n]],
                    regionFlags.CombinationOperator);
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
            (Bitplane)new MmrBitplane(grayScaleHeight, grayScaleWidth):
            new ArithmeticBitplane(
                grayScaleHeight, grayScaleWidth, regionFlags.Template, regionFlags.EnableSkip);
        
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

    private void FillWithBackground(BinaryBitmap targetBitmap)
    {
        if (regionFlags.DefaultPixel) targetBitmap.FillBlack();
    }
}