using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Parsing.StringParsing;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
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
            IPdfObjectParser parser;
            do{}while(source.ShouldContinue(PickParser2(await source.ReadAsync(), out parser!)));

            return await parser!.ParseAsync(source);
        }

        private (bool Success, SequencePosition Position) PickParser2
            (ReadResult source, out IPdfObjectParser? parser)
        {
            var reader = new SequenceReader<byte>(source.Buffer);
            if (!(reader.TryRead(out var firstByte) && reader.TryRead(out var secondByte)))
            {
                parser = null;
                return (false, source.Buffer.Start);
            }
            parser = PickParser( firstByte, secondByte);
            return (true, source.Buffer.Start);
        }

        private static IPdfObjectParser? PickParser(byte firstByte, byte secondByte) =>
            (firstByte, secondByte) switch
            {
                ((byte) '<', (byte) '<') => dictionaryAndStream,
                ((byte) '<', _) => HexString,
                ((byte) '(', _) => SyntaxString,
                ((byte) '[', _) => PdfArray,
                (>= (byte) '0' and <= (byte) '9', _) => Indirects,
                ((byte) '+' or (byte) '-', _) => Number,
                ((byte) '/', _) => Names,
                ((byte) 't', _) => TrueParser,
                ((byte) 'f', _) => FalseParser,
                ((byte) 'n', _) => NullParser,
                ((byte) ']', _) => ArrayTermination,
                ((byte) '>', (byte) '>') => DictionatryTermination,
                _ => throw new PdfParseException($"Unknown Pdf Token {(char)firstByte} {(char)secondByte}")
            };


        private static readonly HexStringParser HexString = new();
        private static readonly SyntaxStringParser SyntaxString = new();
        private static readonly PdfArrayParser PdfArray = new();
        private static readonly PdfDictionaryAndStreamParser dictionaryAndStream = new();
        private static readonly NumberParser Number = new();
        private static readonly NameParser Names = new();
        private static readonly LiteralTokenParser TrueParser = new(PdfBoolean.True);
        private static readonly LiteralTokenParser FalseParser = new(PdfBoolean.False);
        private static readonly LiteralTokenParser NullParser = new(PdfTokenValues.Null);
        private static readonly LiteralTokenParser ArrayTermination = new(PdfTokenValues.ArrayTerminator);
        private static readonly LiteralTokenParser DictionatryTermination = new(PdfTokenValues.DictionaryTerminator);
        //the next line must appear after the declaration of Number in the source file
        private static readonly IndirectObjectParser Indirects = new(Number);
    }
}