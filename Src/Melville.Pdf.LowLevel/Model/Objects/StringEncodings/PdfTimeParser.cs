using System;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

internal readonly ref struct PdfTimeParser
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
