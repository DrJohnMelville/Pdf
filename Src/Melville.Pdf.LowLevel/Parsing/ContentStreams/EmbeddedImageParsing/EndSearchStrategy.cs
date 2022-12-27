using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;

internal class EndSearchStrategy
{
    public static EndSearchStrategy Instance { get; } = new();

    public bool SearchForEndSequence(in BufferFromPipe bfp, out SequencePosition endPos)
    {
        int position = 0;
        var seqReader = bfp.CreateReader();
        if (!SkipBytes(ref seqReader))
        {
            endPos = seqReader.Position;
            return false;
        }

        while (seqReader.TryRead(out var current))
        {
            switch ((char)current, position)
            {
                case ('E', 0):
                    position++;
                    break;
                case ('I', 1):
                    endPos = seqReader.Position;
                    if (VerifyEndPos(seqReader, bfp.Done)) return true;
                    goto default;
                default:
                    position = 0;
                    break;
            }
        }

        endPos = seqReader.Position;
        return false;
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
        position == 0 ? CharClassifier.Classify(current) == CharacterClass.White || current == 0 : IsLegalContentStreamChar(current);

    private bool IsLegalContentStreamChar(byte current) =>
        (Char)current switch
        {
            >= 'A' and <= 'Z' => true,
            >= 'a' and <= 'z' => true,
            >= '0' and <= '9' => true,
            '\0' or '\x09' or '\x0A' or '\x0C' or '\x0D' or '\x20' => true,
            '+' => true,
            '-' => true,
            '*' => true,
            '/' => true,
            '[' => true,
            ']' => true,
            '<' => true,
            '>' => true,
            '.' => true,
            _ => false,
        };
}