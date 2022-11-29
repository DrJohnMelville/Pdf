using System;
using System.Buffers;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public interface IEmbeddedImageTerminationStrategy
{
    bool SearchForEndSequence(in BufferFromPipe bfp, out SequencePosition endPos);
}

[StaticSingleton]
public partial class DiscoverLengthFromContext : IEmbeddedImageTerminationStrategy
{
    public bool SearchForEndSequence(in BufferFromPipe bfp, out SequencePosition endPos)
    {
        int position = 0;
        var seqReader = bfp.CreateReader();
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
    private bool VerifyEndPos(SequenceReader<byte> copiedReader, bool isDone)
    {
        var reader = copiedReader;
        //If the EI we found is really the end of the image, then the text after the EI ought to 
        // look like a content stream, which has a rather restricted syntax.  For right now we just check
        // if the next 20 characters are legal characters in a content stream
        for (int i = 0; i < 20; i++)
        {
            if (!reader.TryRead(out var current)) return isDone;
            if (!IsLegalContentStreamChar((char)current)) return false;
        }

        return true;
    }

    private bool IsLegalContentStreamChar(char current) =>
        current switch
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