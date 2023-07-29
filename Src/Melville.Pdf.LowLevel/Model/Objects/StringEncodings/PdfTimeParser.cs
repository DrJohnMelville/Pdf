using System;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

public readonly ref struct PdfTimeParser
{
    private readonly ReadOnlySpan<byte> source;

    public PdfTimeParser(ReadOnlySpan<byte> source)
    {
        this.source = TrimTerminalApostrophe(source);
    }

    private static ReadOnlySpan<byte> TrimTerminalApostrophe(ReadOnlySpan<byte> span) => 
        span.Length == 0 || span[^1] != (byte)'\'' ? span : span[..^1];

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

    private int TryGetInt(int start, int length, int defaultValue=0) =>
        start + length <= source.Length &&
        NumberTokenizer.TryGetDigitSequence(10, source.Slice(start, length), out var value, out var _) == 0
            ? (int)value
            : defaultValue;
} 
