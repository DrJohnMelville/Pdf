using System;
using System.IO;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.GenericRegionParsers;

public static class TpgdContext
{
    public static ushort Value(
        bool useTpgdon, in BitmapTemplate template, GenericRegionTemplate sourceTemplate) =>
        useTpgdon ? template.ReadContext(LtpContext(sourceTemplate), 2, 3) : (ushort)0;

    private static BinaryBitmap LtpContext(GenericRegionTemplate sourceTemplate)
    {
        return sourceTemplate switch
        {
            GenericRegionTemplate.GB0 => template0,
            GenericRegionTemplate.GB1 => template1,
            GenericRegionTemplate.GB2 => template2,
            GenericRegionTemplate.GB3 => template3,
            _ => throw new InvalidDataException("Unknown source template.")
        };
    }

    private static readonly BinaryBitmap template0 = new(3, 6, new byte[]
    {
        0b010_011,
        0b011_001,
        0b101_000
    });
    private static readonly BinaryBitmap template1 = new(3, 6, new byte[]
    {
        0b000_011,
        0b011_001,
        0b101_000
    });
    private static readonly BinaryBitmap template2 = new(3, 6, new byte[]
    {
        0b000_010,
        0b011_001,
        0b001_000
    });
    private static readonly BinaryBitmap template3 = new(3, 6, new byte[]
    {
        0,
        0b011_001,
        0b101_000
    });
}