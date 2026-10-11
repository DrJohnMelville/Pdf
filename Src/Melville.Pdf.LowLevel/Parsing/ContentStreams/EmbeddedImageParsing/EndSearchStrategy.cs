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
#warning rewrite with searchValues
        var reader = copiedReader;
        //If the EI we found is really the end of the image, then the text after the EI ought to 
        // look like a content stream, which has a rather restricted syntax.  For right now we just check
        // if the next 20 characters are legal characters in a content stream
        for (int i = 0; i < 19; i++)
        {
            if (!reader.TryGetUInt8(out var current)) return isDone;
            if (!IsLegalContentStreamChar(current, i)) return false;
        }
        return true;
    }

    private bool IsLegalContentStreamChar(byte current, int position) =>
        position == 0 ? CharClassifier.IsWhite(current) || current == 0 : IsLegalContentStreamChar(current);

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