using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal class EndSearchStrategy
{
    public static EndSearchStrategy Instance { get; } = new();

    public bool SearchForEndSequence(ReadOnlySpan<byte> data, bool doneReading, out int nextPos)
    {
#warning rewrite with indexOf
        var seqReader = new SpanParsingBuffer(data);
        if (!SkipBytes(ref seqReader))
            return ((int)seqReader.RelativePosition).AsFalseValue(out nextPos);

        int position = 0;

        while (seqReader.TryGetUInt8(out var current))
        {
            switch ((char)current, position)
            {
                case ('E', 0):
                    position++;
                    break;
                case ('I', 1):
                    nextPos  = (int)seqReader.RelativePosition;
                    if (VerifyEndPos(seqReader, doneReading)) return true;
                    goto default;
                default:
                    position = 0;
                    break;
            }
        }

        return ((int)seqReader.RelativePosition).AsFalseValue(out nextPos);
    }

    protected virtual bool SkipBytes(ref SpanParsingBuffer seqReader) => 
        seqReader.TryGetUInt8(out var _);

    private bool VerifyEndPos(SpanParsingBuffer copiedReader, bool isDone)
    {
        if (!isDone && copiedReader.RemainingLength < 20) return false;
        var length = Math.Min(copiedReader.RemainingLength, 20);
        if (length is 0) return true;
        if (!CharacterClassifier.IsWhitespace(copiedReader.Peek()[0])) return false;
        return copiedReader.Peek(20)[1..].IndexOfAnyExcept(LegalContentStreamChars) == -1;
    }

    private static SearchValues<byte> LegalContentStreamChars = SearchValues.Create(
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789\0\x09\x0A\x0C\x0D\x20=-*/[]<>."u8);

    private bool IsLegalContentStreamChar(byte current, int position) =>
        position == 0 ? CharClassifier.IsWhite(current) || current == 0 :
                        IsLegalContentStreamChar(current);

    private bool IsLegalContentStreamChar(byte current) =>
        (char)current  is
            (>= 'A' and <= 'Z') or
            (>= 'a' and <= 'z') or
            (>= '0' and <= '9') or
            '\0' or '\x09' or '\x0A' or '\x0C' or '\x0D' or '\x20' or
            '+' or
            '-' or
            '*' or
            '/' or
            '[' or
            ']' or
            '<' or
            '>' or
            '.';
}