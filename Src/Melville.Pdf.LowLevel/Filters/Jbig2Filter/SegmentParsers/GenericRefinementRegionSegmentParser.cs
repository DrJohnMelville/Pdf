using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Melville.INPC;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public readonly partial struct GenericRefinementRegionFlags
{
    [FromConstructor] private readonly byte data;

    public bool UseGrTemplate1 => BitOperations.CheckBit(data, 0x01);
    public bool UseTpgron => BitOperations.CheckBit(data, 0x02);
}

public readonly struct GenericRefinementRegionSegmentParser
{
    public static GenericRefinementRegionSegment Parse(
        SequenceReader<byte> reader, ReadOnlySpan<Segment> referencedSegments)
    {
        var regionHead = RegionHeaderParser.Parse(ref reader);
        var flags = new GenericRefinementRegionFlags(reader.ReadBigEndianUint8());
        var template = new RefinementTemplateSet(ref reader, flags.UseGrTemplate1);

        var bitmap = regionHead.CreateTargetBitmap();
        new GenericRegionRefinementAlgorithm(bitmap, GetReferenceBitmap(referencedSegments),
            template, new MQDecoder(), LtpContext(flags)).Read(ref reader);

        return new GenericRefinementRegionSegment(SegmentType.ImmediateGenericRefinementRegion,
            regionHead, bitmap);
    }

    private static ushort LtpContext(GenericRefinementRegionFlags flags) =>
        (flags.UseTpgron, flags.UseGrTemplate1) switch
        {
            (false, _) => 0,
            (true, false) => 0b000010000_0000,
            (true, true) => 0b001000_0000
        };

    private static BinaryBitmap GetReferenceBitmap(ReadOnlySpan<Segment> referencedSegments)
    {
        Debug.Assert(referencedSegments.Length == 1);
        var refBitmap = (referencedSegments[0] as RegionSegment)?.Bitmap ??
                        throw new InvalidDataException("Cannot find ReferenceBitmap");
        return refBitmap;
    }
}