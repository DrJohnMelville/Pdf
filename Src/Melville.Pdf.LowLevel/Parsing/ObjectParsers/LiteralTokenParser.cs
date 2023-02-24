using System;
using System.Buffers;
using System.Runtime.InteropServices.ComTypes;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class LiteralTokenParser : PdfAtomParser
{
    private PdfTokenValues literal;

    public LiteralTokenParser(PdfTokenValues literal)
    {
        this.literal = literal;
    }

    public override bool TryParse(
        ref SequenceReader<byte> reader, bool final, IParsingReader source, out PdfObject obj)
    {
        obj = literal;
        if (!literal.TokenValue.CheckReaderFor(ref reader, final, out var correct)) return false;
        if (!correct) throw new PdfParseException("Unexpected PDF token.");
        return true;
    }
}