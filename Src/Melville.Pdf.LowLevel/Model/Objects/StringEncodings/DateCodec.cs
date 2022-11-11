using System;
using Melville.Pdf.LowLevel.Model.Primitives;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    public override string ToString() => $"{DateTime} {HourOffset}:{MinuteOffset}";
}

public ref struct PdfTimeParser
{
    private readonly ReadOnlySpan<char> source;

    public PdfTimeParser(ReadOnlySpan<char> source)
    {
        this.source = TrimTerminalApostrophe(source);
    }

    private static ReadOnlySpan<char> TrimTerminalApostrophe(ReadOnlySpan<char> span) => 
        span.Length == 0 || span[^1] != '\'' ? span : span[..^1];
     public  PdfTime AsPdfTime() =>
        new( new DateTime(
                TryGetInt(2, 4, 1), // Year 
                TryGetInt(6, 2, 1),  // Month
                TryGetInt(8, 2, 1),  // Day
                TryGetInt(10, 2),    // Hour
                TryGetInt(12, 2),    // Minute
                TryGetInt(14, 2)),   // Second
            TryGetSign(16) * TryGetInt(17, 2), // Hour Offset 
            TryGetInt(20, 2)); // MinuteOffset

    private int TryGetSign(int i) => 
        i < source.Length && source[i] == '-' ? -1 : 1;

    private int TryGetInt(int start, int length, int defaultValue=0)
    {
        if (start + length > source.Length ||
            !int.TryParse(source.Slice(start, length), out var value)) return defaultValue;
        return value;
    }} 
