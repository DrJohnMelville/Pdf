using System;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

/// <summary>
/// Represents a date and time in a given offset from UTC
/// </summary>
public readonly struct PdfTime
{
    /// <summary>
    /// The date and time represented
    /// </summary>
    public DateTime DateTime { get; }
    /// <summary>
    /// Hours offset from UTC
    /// </summary>
    public int HourOffset { get; }
    /// <summary>
    /// Minutes offset from UTC
    /// </summary>
    public int MinuteOffset { get; }

    /// <summary>
    /// Create a PdfTime
    /// </summary>
    /// <param name="dateTime">The date ant time represented</param>
    /// <param name="hourOffset">Offset in hours from UTC, defaults to 0</param>
    /// <param name="minuteOffset">Minutes part of the offset from UTC, defaults to 0</param>
    public PdfTime(DateTime dateTime, int hourOffset = 0, int minuteOffset = 0)
    {
        DateTime = dateTime;
        HourOffset = hourOffset;
        MinuteOffset = minuteOffset;
    }

    /// <summary>
    /// This implicit conversion operator allows a datatime to be used when a PdfTime is expected
    /// </summary>
    /// <param name="source"></param>
    public static implicit operator PdfTime(DateTime source) => new(source);

    /// <summary>
    /// Format the PdfTime into the shortest possible PDF representation -- exploiting all default values
    /// </summary>
    /// <returns>The time represented by this struct as a PDF time string</returns>
    public byte[] AsPdfBytes()
    {
        var lastField = LastNonDefaultElement();
        Span<byte> buffer = stackalloc byte[22];
        buffer[0] = (byte)'D';
        buffer[1] = (byte)':';
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[2..], DateTime.Year, 4);
        if (lastField == DateTimeMember.Year) return buffer[..6].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[6..8], DateTime.Month, 2);
        if (lastField == DateTimeMember.Month) return buffer[..8].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[8..], DateTime.Day, 2);
        if (lastField == DateTimeMember.Day) return buffer[..10].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[10..], DateTime.Hour, 2);
        if (lastField == DateTimeMember.Hour) return buffer[..12].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[12..], DateTime.Minute, 2);
        if (lastField == DateTimeMember.Minute) return buffer[..14].ToArray();
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[14..], DateTime.Second, 2);
        if (lastField == DateTimeMember.Second) return buffer[..16].ToArray();
        buffer[16] = (byte) (HourOffset < 0?'-':'+');
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[17..], Math.Abs(HourOffset), 2);
        if (lastField == DateTimeMember.HourOffset) return buffer[..19].ToArray();
        buffer[19] = (byte)'\'';
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[20..], Math.Abs(MinuteOffset), 2);
        return buffer.ToArray();
    }
    private DateTimeMember LastNonDefaultElement()
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