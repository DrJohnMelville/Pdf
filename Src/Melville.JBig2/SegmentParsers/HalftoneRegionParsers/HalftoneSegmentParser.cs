using System;
using System.Buffers;
using System.IO;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.SegmentParsers.HalftoneRegionParsers;

public readonly struct HalftoneRegionFlags
{
    private readonly byte data;

    public HalftoneRegionFlags(byte data)
    {
        this.data = data;
    }

    /// <summary>
    /// In spec HMMR
    /// </summary>
    public bool UseMMR => BitOperations.CheckBit(data, 0x01);
    /// <summary>
    /// In spec HTEMPLATE
    /// </summary>
    public GenericRegionTemplate Template => 
        (GenericRegionTemplate)BitOperations.UnsignedInteger(data, 1, 0b11);
    /// <summary>
    /// In Spec HENABLESKIP
    /// </summary>
    public bool EnableSkip => BitOperations.CheckBit(data, 0x08);
    public CombinationOperator CombinationOperator =>
        (CombinationOperator)BitOperations.UnsignedInteger((int)data, 4, 7);
    /// <summary>
    /// In Spec HDDEFPIXEL
    /// </summary>
    public bool DefaultPixel => BitOperations.CheckBit(data, 0x80);
        
}

public static class HalftoneSegmentParser
{
    public static HalftoneSegment Parse(SequenceReader<byte> reader, in ReadOnlySpan<Segment> segs)
    {
        var halfToneWriter = new HalftoneSegmentReader(
            RegionHeaderParser.Parse(ref reader),
            new HalftoneRegionFlags(reader.ReadBigEndianUint8()),
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianUint32(),
            reader.ReadBigEndianInt32(),
            reader.ReadBigEndianInt32(),
            reader.ReadBigEndianUint16(),
            reader.ReadBigEndianUint16(),
            GetHalftoneDictionary(segs));
        var bitmap = halfToneWriter.Header.CreateTargetBitmap();
        try
        { 
            halfToneWriter.ReadBitmap(ref reader, bitmap);
        }
        catch (InvalidDataException)
        {
        }
        return new HalftoneSegment(SegmentType.ImmediateHalftoneRegion, halfToneWriter.Header, bitmap);
    }

    private static DictionarySegment GetHalftoneDictionary(ReadOnlySpan<Segment> segs) => 
        segs[0] as DictionarySegment ?? 
        throw new InvalidDataException("Need a dictionary for a halftoneregion");

}