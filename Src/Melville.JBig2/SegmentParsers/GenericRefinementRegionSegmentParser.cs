using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using Melville.INPC;
using Melville.JBig2.ArithmeticEncodings;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.GenericRegionRefinements;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.SegmentParsers;

internal readonly partial struct GenericRefinementRegionFlags
{
    [FromConstructor] private readonly byte data;

    public bool UseGrTemplate1 => BitOperations.CheckBit(data, 0x01);
    public bool UseTpgron => BitOperations.CheckBit(data, 0x02);
}

internal readonly struct GenericRefinementRegionSegmentParser
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

    private static int LtpContext(GenericRefinementRegionFlags flags) =>
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