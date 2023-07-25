using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class StringDecoderParser<T, TState> : PdfAtomParser
    where T : IStringDecoder<TState>, new() where TState : new()
{
    public override bool TryParse(
        ref SequenceReader<byte> reader, bool final, IParsingReader source,
        [NotNullWhen(true)] out PdfObject? obj)
    {
        reader.Advance(1);
        if (!new StringTokenizer<T, TState>()
                .Parse(ref reader, out byte[]? stringBytes))
        {
            if (final)
                throw new PdfParseException("Unterminated String");
            return PdfTokenValues.Null.AsFalseValue(out obj);
        }
//        obj = source.CreateDecryptedString(stringBytes);
        obj = null;
        return true;
    }
}