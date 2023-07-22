namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

/// <summary>
/// The parts of a pdf date time, the value is the minimum length of a
/// PdfString w here that value is not zero
/// </summary>
internal enum DateTimeMember
{
    Year = 6,
    Month = 8,
    Day = 10,
    Hour = 12,
    Minute = 14,
    Second = 16,
    HourOffset = 19,
    MinuteOffset = 22
}

internal static class DateTimeMemberOperations
{
    public static int LengthOfPdfTimeRepresentation(this DateTimeMember member) =>
        (int)member;
}