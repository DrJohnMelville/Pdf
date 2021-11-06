using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public ref struct DictionarySkipper
{
    private SequenceReader<byte> source;
    private byte current;
    private byte prior;

    public DictionarySkipper(ref SequenceReader<byte> source)
    {
        this.source = source;
        source.TryPeek(out prior);
        source.TryPeek(1, out current);
    }

    private bool Advance()
    {
        source.TryRead(out prior);
        prior = current;
        return source.TryPeek(out current);
    }

    public bool TrySkipDictionary()
    {
        if (!Check('<', '<')) throw new PdfParseException("Trying to skip a non dictionary");
        if (!(Advance()&&Advance())) return false;
        while (true)
        {
            switch ((char)prior, (char) current)
            {
                case ('>', '>'):
                    return Advance() && Advance();
                case ('<','<'):
                    TrySkipDictionary();
                    break;
                default:
                    if (!Advance()) return false;
                    break;
            }
        }
    }
    
    private bool Check(char expectedPrior, char expectedCurrent) => 
        expectedCurrent == current && expectedPrior == prior;

    public SequencePosition CurrentPosition => source.Position;
}