using System;
using System.Buffers;
using System.Net.Http.Headers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal class EndSearchStrategy
{
    public static EndSearchStrategy Instance { get; } = new();

    public bool SearchForEndSequence(in BufferFromPipe bfp, out SequencePosition endPos)
    {
        var seqReader = bfp.CreateReader();
        return SearchForEndSequence(seqReader, bfp.Done, out endPos);
    }

    public bool SearchForEndSequence(SequenceReader<byte> seqReader, bool sourceDone, out SequencePosition endPos)
    {
        int position = 0;
        if (!SkipBytes(ref seqReader))
            return seqReader.Position.AsFalseValue(out endPos);
        while (seqReader.TryRead(out var current))
        {
            switch ((char)current, position)
            {
                case ('E', 0):
                    position++;
                    break;
                case ('I', 1):
                    endPos = seqReader.Position;
                    if (VerifyEndPos(seqReader, sourceDone)) return true;
                    goto default;
                default:
                    position = 0;
                    break;
            }
        }

        return seqReader.Position.AsFalseValue(out endPos);
    }

    protected virtual bool SkipBytes(ref SequenceReader<byte> seqReader) => seqReader.TryRead(out _);

    private bool VerifyEndPos(SequenceReader<byte> copiedReader, bool isDone)
    {
        var reader = copiedReader;
        //If the EI we found is really the end of the image, then the text after the EI ought to 
        // look like a content stream, which has a rather restricted syntax.  For right now we just check
        // if the next 20 characters are legal characters in a content stream
        for (int i = 0; i < 19; i++)
        {
            if (!reader.TryRead(out var current)) return isDone;
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