using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.GenericRegionParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.HalftoneRegionParsers;

public ref struct HalftoneSegmentReader
{
    private readonly RegionHeader regionHeader;
    private readonly HalftoneRegionFlags regionFlags;
    private readonly uint grayScaleWidth; // HGW
    private readonly uint grayScaleHeight; // HGW
    private readonly int grayScaleXOffset; // HGX
    private readonly int grayScaleYOffset; // HGY
    private readonly uint vectorX; // HRY
    private readonly uint vectorY; // HRY
    private readonly DictionarySegment dictionary;

    // order of arguments is correctness critial because HalftoneSegmentParser uses this order to
    // read data out of the bitstream
    public HalftoneSegmentReader(RegionHeader regionHeader, HalftoneRegionFlags regionFlags, uint grayScaleWidth, uint grayScaleHeight, int grayScaleXOffset, int grayScaleYOffset, uint vectorX, uint vectorY, DictionarySegment dictionary)
    {
        this.regionHeader = regionHeader;
        this.regionFlags = regionFlags;
        this.grayScaleWidth = grayScaleWidth;
        this.grayScaleHeight = grayScaleHeight;
        this.grayScaleXOffset = grayScaleXOffset;
        this.grayScaleYOffset = grayScaleYOffset;
        this.vectorX = vectorX;
        this.vectorY = vectorY;
        this.dictionary = dictionary;
    }

    public unsafe HalftoneSegment ReadSegment(ref SequenceReader<byte> reader)
    {
        var targetBitmap = regionHeader.CreateTargetBitmap();
        FillWithBackground(targetBitmap);

        var spanLength = (int)(grayScaleHeight * grayScaleWidth);
        var greyScaleBitmapPointer = stackalloc int[spanLength];
        var grayScaleBitmapSpan = new Span<int>(greyScaleBitmapPointer, spanLength);
        var gsb = ReadGrayScaleBitmap(ref reader, grayScaleBitmapSpan);

        #warning  this is horibly inefficient and needs some love
        for (int m = 0; m < grayScaleHeight; m++)
        {
            for (int n = 0; n < grayScaleWidth; n++)
            {
                var x = (grayScaleXOffset + m * vectorY + n * vectorX) >> 8;
                var y = (grayScaleYOffset + m * vectorX - n * vectorY) >> 8;
                targetBitmap.PasteBitsFrom((int)y,(int)x,dictionary.ExportedSymbols.Span[gsb[m,n]], 
                    regionFlags.CombinationOperator);
            }
        }
        
        return new HalftoneSegment(SegmentType.ImmediateLosslessHalftoneRegion, regionHeader,
            targetBitmap);
    }

    private GrayScaleBitmap ReadGrayScaleBitmap(ref SequenceReader<byte> reader, Span<int> data)
    {
        var gsb = new GrayScaleBitmap(data, (int)grayScaleWidth);
        var bitPlane = new BinaryBitmap((int)grayScaleHeight, (int)grayScaleWidth);
        var genericRegionReader = new GenericRegionReader(bitPlane, regionFlags.UseMMR);

        var bitsPerValue = IntLog.CeilingLog2Of((uint)dictionary.ExportedSymbols.Length);
        var bitValue = 1 << (bitsPerValue - 1);
        genericRegionReader.ReadFrom(ref reader, true);
        gsb.CopyBinaryBitmap(bitPlane, bitValue);
        var priorBitValue = bitValue;
        bitValue >>= 1;
        for (; bitValue > 0; bitValue >>= 1, priorBitValue >>= 1)
        {
            bitPlane.FillBlack();
            genericRegionReader.ReadFrom(ref reader, true);
            gsb.ProcessBinaryBitmap(bitPlane, bitValue, priorBitValue);
        }

        return gsb;
    }

    private void FillWithBackground(BinaryBitmap targetBitmap)
    {
        if (regionFlags.DefaultPixel) targetBitmap.FillBlack();
    }
}