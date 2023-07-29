using System;
using System.Diagnostics;
using System.Text;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

public static class PdfTimeExtenstions
{
    public static PdfTime AsPdfTime(this in PdfDirectValue value)
    {
        var sourceSpan = (ReadOnlySpan<byte>)value.Get<StringSpanSource>().GetSpan();
        Span<char> decoded = stackalloc char[sourceSpan.DecodedLength()];
        sourceSpan.FillDecodedChars(decoded);

        Span<byte> reEncoded = stackalloc byte[Encoding.UTF8.GetByteCount(decoded)];
        Encoding.UTF8.GetBytes(decoded, reEncoded);

        return new PdfTimeParser(reEncoded).AsPdfTime();
    }

}

public static class StringDecodingImpl
{
    public static int DecodedLength(this ReadOnlySpan<byte> input)
    {
        var (encoding, length) = ByteOrderDetector.DetectByteOrder(input);
        return encoding.GetDecoder().GetCharCount(input[length..], true);
    }

    public static void FillDecodedChars(this ReadOnlySpan<byte> input, Span<char> output)
    {
        var (encoding, length) = ByteOrderDetector.DetectByteOrder(input);
        encoding.GetDecoder().GetChars(input[length..], output, true);
    }
}

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
    public PdfDirectValue AsPdfBytes() => 
        PdfDirectValue.CreateString(FillWithTime(stackalloc byte[22]));

    private Span<byte> FillWithTime(in Span<byte> buffer)
    {
        var lastElement = LastNonDefaultElement();
        Debug.Assert(buffer.Length >= (int)lastElement);
        switch (lastElement)
        {
            case DateTimeMember.MinuteOffset:
                buffer[19] = (byte)'\'';
                WriteNumber(buffer, 20, 2, Math.Abs(MinuteOffset));
                goto case DateTimeMember.HourOffset;
            case DateTimeMember.HourOffset:
                buffer[16] = (byte) (HourOffset < 0?'-':'+');
                WriteNumber(buffer, 17, 2, Math.Abs(HourOffset));
                goto case DateTimeMember.Second;
            case DateTimeMember.Second:
                WriteNumber(buffer, 14, 2, DateTime.Second);
                goto case DateTimeMember.Minute;
            case DateTimeMember.Minute:
                WriteNumber(buffer, 12, 2, DateTime.Minute);
                goto case DateTimeMember.Hour;
            case DateTimeMember.Hour:
                WriteNumber(buffer, 10, 2, DateTime.Hour);
                goto case DateTimeMember.Day;
            case DateTimeMember.Day:
                WriteNumber(buffer, 8, 2, DateTime.Day);
                goto case DateTimeMember.Month;
            case DateTimeMember.Month:
                WriteNumber(buffer, 6, 2, DateTime.Month);
                goto case DateTimeMember.Year;
            case DateTimeMember.Year:
                WriteNumber(buffer, 2, 4, DateTime.Year);
                buffer[0] = (byte)'D';
                buffer[1] = (byte)':';
                return buffer[..lastElement.LengthOfPdfTimeRepresentation()];
            default: goto case DateTimeMember.MinuteOffset;
        }
    }

    private void WriteNumber(Span<byte> buffer, int position, int length, int value) => 
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer[position..], value, length);

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

    /// <inheritdoc />
    public override string ToString() => $"{DateTime} {HourOffset}:{MinuteOffset}";
}