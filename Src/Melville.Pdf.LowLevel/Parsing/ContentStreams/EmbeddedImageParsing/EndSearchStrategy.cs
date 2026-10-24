using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal class EndSearchStrategy
{
    public static EndSearchStrategy Instance { get; } = new();

    public bool SearchForEndSequence(ReadOnlySpan<byte> data, bool doneReading, out int nextPos)
    {
        var seqReader = new SpanParsingBuffer(data);
        if (!SkipBytes(ref seqReader))
            return ((int)seqReader.RelativePosition).AsFalseValue(out nextPos);

        while (seqReader.CurrentLength> 0)
        {
            var index = seqReader.Peek().IndexOf("EI"u8);
            if (index < 0) return 0.AsFalseValue(out nextPos);
            seqReader.Advance(index + 2);
            if (VerifyEndPos(seqReader, doneReading))
                return ((int)seqReader.RelativePosition).AsTrueValue(out nextPos);
        }
        return 0.AsFalseValue(out nextPos);
    }

    protected virtual bool SkipBytes(ref SpanParsingBuffer seqReader) => 
        seqReader.TryGetUInt8(out var _);

    private bool VerifyEndPos(SpanParsingBuffer copiedReader, bool isDone) =>
        copiedReader.Peek().TakeMax(20) switch
        {
            { Length: < 20 } when !isDone => false,
            [] => true,
            [var delimiter, .. var tail] => CharacterClassifier.IsWhitespace(delimiter) &&
                          tail.IndexOfAnyExcept(LegalContentStreamChars) == -1
        };

    private static SearchValues<byte> LegalContentStreamChars = SearchValues.Create(
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789\0\x09\x0A\x0C\x0D\x20=-*/[]<>."u8);
}