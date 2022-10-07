using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
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
        await NextTokenFinder.SkipToNextToken(source.Reader).CA();
        IPdfObjectParser parser;
        do
        {
        } while (source.Reader.Source.ShouldContinue(PickParser(await source.Reader.Source.ReadAsync().CA(), out parser!)));

        return await parser.ParseAsync(source).CA();
    }

    private (bool Success, SequencePosition Position) PickParser
        (ReadResult source, out IPdfObjectParser? parser)
    {
        var reader = new SequenceReader<byte>(source.Buffer);
        if (TryReadTwoBytes(ref reader, out var firstByte, out var secondByte) ||
            source.IsCompleted)
        {
            parser = PickParserOverride((char)firstByte, (char)secondByte);
            return (true, source.Buffer.Start);
        }
        parser = null;
        return (false, source.Buffer.Start);
    }

    private bool TryReadTwoBytes(
        ref SequenceReader<byte> source, out byte firstByte, out byte secondByte)
    {
        if (source.TryRead(out firstByte))
            if (source.TryRead(out secondByte))
                return true;
        secondByte = 0;
        return false;
    }

    protected virtual IPdfObjectParser? PickParserOverride(char firstByte, char secondByte) =>
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

public class InlineImageNameParser : PdfCompositeObjectParserBase
{
    private static readonly LiteralTokenParser term = new(PdfTokenValues.InlineImageDictionaryTerminator);
    
    protected override IPdfObjectParser? PickParserOverride(char firstByte, char secondByte) =>
        (firstByte, secondByte) switch
        {
            ('I', 'D') => term, 
            _ => base.PickParserOverride(firstByte, secondByte)
        };
}

public class ExpandSynonymsParser : IPdfObjectParser
{
    private readonly IPdfObjectParser inner;
    private IDictionary<PdfObject, PdfObject> expansions;

    public ExpandSynonymsParser(IPdfObjectParser inner, IDictionary<PdfObject, PdfObject> expansions)
    {
        this.inner = inner;
        this.expansions = expansions;
    }

    public async Task<PdfObject> ParseAsync(IParsingReader source)
    {
        var ret = await inner.ParseAsync(source).CA();
        return expansions.TryGetValue(ret, out var expansion) ? expansion : ret;
    }
}

public class PdfCompositeObjectParser : PdfCompositeObjectParserBase
{
    protected override IPdfObjectParser? PickParserOverride(char firstByte, char secondByte) =>
        (firstByte, secondByte) switch
        {
            ('<', '<') =>  PdfParserParts.dictionaryAndStream,
            (>= '0' and <= '9', _) =>  PdfParserParts.Indirects,
            _ => base.PickParserOverride(firstByte, secondByte)
        };
}