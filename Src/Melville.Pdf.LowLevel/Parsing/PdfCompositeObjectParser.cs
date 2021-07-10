using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Parsing.StringParsing;

namespace Melville.Pdf.LowLevel.Parsing
{
    public interface IPdfObjectParser
    {
        public Task<PdfObject> ParseAsync(ParsingSource source);

    }


    public class PdfCompositeObjectParser:  IPdfObjectParser
    {
        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            await NextTokenFinder.SkipToNextToken(source);
            while (true)
            {
                var data = await source.ReadAsync();
                var parser = PickParser(data.Buffer);
                if (parser != null)
                {
                    source.AdvanceTo(data.Buffer.Start, data.Buffer.GetPosition(2));
                    return await parser.ParseAsync(source);
                }
                source.NeedMoreInputToAdvance();
            }
        }

        private IPdfObjectParser? PickParser(ReadOnlySequence<byte> dataBuffer)
        {
            var reader = new SequenceReader<byte>(dataBuffer);
            if (!(reader.TryRead(out var firstByte) && reader.TryRead(out var secondByte)))
            {
                return null;
            }
            return (firstByte, secondByte) switch
            {
                ((byte) '<', (byte)'<') => dictionaryAndStream,
                ((byte) '<', _) => HexString,
                ((byte) '(', _) => SyntaxString,
                ((byte) '[', _) => PdfArray,
                ( >= (byte) '0' and <= (byte)'9', _) => Indirects,
                ((byte) '+' or (byte)'-', _) => Number,
                ((byte) '/', _) => Names,
                ((byte) 't', _) => TrueParser,
                ((byte) 'f', _) => FalseParser,
                ((byte) 'n', _) => NullParser,
                ((byte) ']', _) => ArrayTermination,
                ((byte) '>', (byte)'>') => DictionatryTermination,
                _ => throw new PdfParseException("Unknown Pdf Token")
            };
        }

        private static readonly HexStringParser HexString = new();
        private static readonly SyntaxStringParser SyntaxString = new();
        private static readonly PdfArrayParser PdfArray = new();
        private static readonly PdfDictionaryAndStreamParser dictionaryAndStream = new();
        private static readonly NumberParser Number = new();
        private static readonly NameParser Names = new();
        private static readonly LiteralTokenParser TrueParser = new(4, PdfBoolean.True);
        private static readonly LiteralTokenParser FalseParser = new(5, PdfBoolean.False);
        private static readonly LiteralTokenParser NullParser = new(4, PdfEmptyConstants.Null);
        private static readonly LiteralTokenParser ArrayTermination = new(1, PdfEmptyConstants.ArrayTerminator);
        private static readonly LiteralTokenParser DictionatryTermination = new(2, PdfEmptyConstants.DictionaryTerminator);
        //the next line must appear after the declaration of Number in the source file
        private static readonly IndirectObjectParser Indirects = new(Number);
    }
}