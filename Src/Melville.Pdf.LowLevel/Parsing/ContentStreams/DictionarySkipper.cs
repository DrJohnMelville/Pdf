using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public ref struct DictionarySkipper
{
    private SequenceReader<byte> source;
    private byte next;
    private byte current;

    public DictionarySkipper(ref SequenceReader<byte> source)
    {
        this.source = source;
        source.TryPeek(out current);
        source.TryPeek(1, out next);
    }

    private bool Advance()
    {
        source.TryRead(out current);
        current = next;
        return source.TryPeek(1, out next);
    }

    public bool TrySkipDictionary()
    {
        if (!Check('<', '<')) throw new PdfParseException("Trying to skip a non dictionary");
        if (!(Advance()&&Advance())) return false;
        while (true)
        {
            switch ((char)current, (char) next)
            {
                case ('>', '>'):
                    return Advance() && Advance();
                case ('<','<'):
                    if (!TrySkipDictionary()) return false;
                    break;
                case ('<', _):
                    if (!TrySkipHexString()) return false;
                    break;
                case ('(',_):
                    if (!TrySkipSyntaxString()) return false;
                    break;
                default:
                    if (!Advance()) return false;
                    break;
            }
        }
    }

    private bool TrySkipHexString()
    {
        while (current != (byte)'>')
        {
            if (!Advance()) return false;
        }
        return Advance();
    }

    private bool TrySkipSyntaxString()
    {
        Advance();
        while (true)
        {
            switch ((char)current)
            {
                case '\\':
                    if (!(Advance() && Advance())) return false;
                    break;
                case ')':
                    return Advance();
                case '(':
                    if (!TrySkipSyntaxString()) return false;
                    break;
                default:
                    if (!Advance()) return false;
                    break;
            }
        }
    }

    private bool Check(char expectedPrior, char expectedCurrent) => 
        expectedCurrent == next && expectedPrior == current;

    public SequencePosition CurrentPosition => source.Position;
}