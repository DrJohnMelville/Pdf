using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

public enum DateTimeMember
{
    Year,
    Month,
    Day,
    Hour,
    Minute,
    Second,
    HourOffset,
    MinuteOffset
}
public readonly struct PdfTime
{
    public DateTime DateTime { get; }
    public int HourOffset { get; }
    public int MinuteOffset { get; }

    public PdfTime(DateTime dateTime, int hourOffset = 0, int minuteOffset = 0)
    {
        DateTime = dateTime;
        HourOffset = hourOffset;
        MinuteOffset = minuteOffset;
    }

    public static implicit operator PdfTime(DateTime source) => new(source);

    public DateTimeMember LastNonDefaultElement()
    {
        if (MinuteOffset != 0) return DateTimeMember.MinuteOffset;
        if (HourOffset != 0) return DateTimeMember.HourOffset;
        if (DateTime.Second != 0) return DateTimeMember.Second;
        if (DateTime.Minute != 0) return DateTimeMember.Minute;
        if (DateTime.Hour != 0) return DateTimeMember.Hour;
        if (DateTime.Day != 1) return DateTimeMember.Day;
        if (DateTime.Month != 1) return DateTimeMember.Month;
        return DateTimeMember.Year;
    }

    public override string ToString() => $"{DateTime} {HourOffset}:{MinuteOffset}";
}
public static class DateCodec
{
    public static PdfTime AsPdfTime(this byte[] source) =>
        new( new DateTime(
                TryGetInt(source, 2, 4, 1), // Year 
                TryGetInt(source, 6, 2, 1),  // Month
                TryGetInt(source, 8, 2, 1),  // Day
                TryGetInt(source, 10, 2),    // Hour
                TryGetInt(source, 12, 2),    // Minute
                TryGetInt(source, 14, 2)),   // Second
            TryGetSign(source, 16) * TryGetInt(source, 17, 2), // Hour Offset 
            TryGetInt(source, 20, 2)); // MinuteOffset

    private static int TryGetSign(byte[] source, int i) => 
        i < source.Length && source[i] == '-' ? -1 : 1;

    private static int TryGetInt(byte[] source, int start, int length, int defaultValue=0)
    {
        if (start + length > source.Length) return defaultValue;
        return (int)WholeNumberParser.ParsePositiveWholeNumber(source.AsSpan(start, length));
    }

    public static byte[] AsPdfBytes(this PdfTime source)
    {
        var lastField = source.LastNonDefaultElement();
        Span<byte> buffer = stackalloc byte[22];
        buffer[0] = (byte)'D';
        buffer[1] = (byte)':';
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[2..], source.DateTime.Year, 4);
        if (lastField == DateTimeMember.Year) return buffer[..6].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[6..8], source.DateTime.Month, 2);
        if (lastField == DateTimeMember.Month) return buffer[..8].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[8..], source.DateTime.Day, 2);
        if (lastField == DateTimeMember.Day) return buffer[..10].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[10..], source.DateTime.Hour, 2);
        if (lastField == DateTimeMember.Hour) return buffer[..12].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[12..], source.DateTime.Minute, 2);
        if (lastField == DateTimeMember.Minute) return buffer[..14].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[14..], source.DateTime.Second, 2);
        if (lastField == DateTimeMember.Second) return buffer[..16].ToArray();
        buffer[16] = (byte) (source.HourOffset < 0?'-':'+');
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[17..], Math.Abs(source.HourOffset), 2);
        if (lastField == DateTimeMember.HourOffset) return buffer[..19].ToArray();
        buffer[19] = (byte)'\'';
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[20..], Math.Abs(source.MinuteOffset), 2);
        return buffer.ToArray();
    }
        
}