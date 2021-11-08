using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Primitives.PipeReaderWithPositions;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public interface IPdfObjectParser
{
    public Task<PdfObject> ParseAsync(IParsingReader source);

}


public class PdfCompositeObjectParserBase : IPdfObjectParser
{
    public async Task<PdfObject> ParseAsync(IParsingReader source)
    {
        await NextTokenFinder.SkipToNextToken(source.Reader);
        IPdfObjectParser parser;
        do
        {
        } while (source.Reader.ShouldContinue(PickParser2(await source.Reader.ReadAsync(), out parser!)));

        return await parser.ParseAsync(source);
    }

    private (bool Success, SequencePosition Position) PickParser2
        (ReadResult source, out IPdfObjectParser? parser)
    {
        var reader = new SequenceReader<byte>(source.Buffer);
        if (reader.TryRead(out var firstByte))
        {
            if (reader.TryRead(out var secondByte))
            {
                parser = PickParser((char)firstByte, (char)secondByte);
                return (true, source.Buffer.Start);
            }
            if (source.IsCompleted && firstByte == ']')
            {
                parser = PdfParserParts.ArrayTermination;
                return (true, source.Buffer.Start);
            }
        }

        parser = null;
        return (false, source.Buffer.Start);
    }

    protected virtual IPdfObjectParser? PickParser(char firstByte, char secondByte) =>
        (firstByte, secondByte) switch
        {
            ('<', '<') =>  PdfParserParts.Dictionary,
            ('<', _) =>  PdfParserParts.HexString,
            ('(', _) =>  PdfParserParts.SyntaxString,
            ('[', _) => PdfParserParts.PdfArray,
            ((>= '0' and <= '9') or '+' or '-' or '.', _) =>  PdfParserParts.Number,
            ('/', _) =>  PdfParserParts.Names,
            ('t', _) =>  PdfParserParts.TrueParser,
            ('f', _) =>  PdfParserParts.FalseParser,
            ('n', _) =>  PdfParserParts.NullParser,
            (']', _) =>  PdfParserParts.ArrayTermination,
            ('>', '>') =>  PdfParserParts.DictionatryTermination,
            _ => throw new PdfParseException($"Unknown Pdf Token {firstByte}{secondByte}")
        };
}

public class PdfCompositeObjectParser : PdfCompositeObjectParserBase
{
    protected override IPdfObjectParser? PickParser(char firstByte, char secondByte) =>
        (firstByte, secondByte) switch
        {
            ('<', '<') =>  PdfParserParts.dictionaryAndStream,
            (>= '0' and <= '9', _) =>  PdfParserParts.Indirects,
            _ => base.PickParser(firstByte, secondByte)
        };
}