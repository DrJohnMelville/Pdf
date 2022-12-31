using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class NumberParser: PdfAtomParser
{
    public override bool TryParse(
        ref SequenceReader<byte> reader, bool final, IParsingReader source, 
        [NotNullWhen(true)] out PdfObject? obj)
    {
        var parser = new NumberWithFractionParser();
        if (parser.InnerTryParse(ref reader, final))
        {
            obj = parser.HasFractionalPart()
                ? parser.DoubleValue()
                : parser.IntegerValue();
            return true;
        };
        obj = PdfTokenValues.Null;
        return false;
    }
}