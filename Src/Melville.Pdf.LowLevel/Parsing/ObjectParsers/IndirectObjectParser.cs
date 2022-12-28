using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class IndirectObjectParser : IPdfObjectParser
{
    private readonly IPdfObjectParser fallbackNumberParser;

    public IndirectObjectParser(IPdfObjectParser fallbackNumberParser)
    {
        this.fallbackNumberParser = fallbackNumberParser;
    }

    public async Task<PdfObject> ParseAsync(IParsingReader source)
    {
        ParseResult kind;
        int num = 0;
        int generation = 0;
        do{}while(source.Reader.ShouldContinue(ParseReference(await source.Reader.ReadAsync().CA(), out kind, out num, out  generation)));

        switch (kind)
        {
            case ParseResult.FoundReference:
                return source.IndirectResolver.FindIndirect(num, generation);;
                
            case ParseResult.FoundDefinition:
                do { } while (source.Reader.ShouldContinue(SkipToObjectBeginning(await source.Reader.ReadAsync().CA())));
                var target = await source.RootObjectParser.ParseAsync(source).CA();
                return target;
                
            case ParseResult.NotAReference:
                return await fallbackNumberParser.ParseAsync(source).CA();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private (bool Success, SequencePosition Position) SkipToObjectBeginning(ReadResult source)
    {
        var reader = new SequenceReader<byte>(source.Buffer);
        return (reader.TryAdvance(2) && NextTokenFinder.SkipToNextToken(ref reader), reader.Position);

    }
    private enum ParseResult
    {
        FoundReference,
        FoundDefinition,
        NotAReference,
    }
        
        
    private (bool, SequencePosition) ParseReference(ReadResult rr, out ParseResult kind,
        out  int num, out int generation)
    {
        var reader = new SequenceReader<byte>(rr.Buffer);
        kind = ParseResult.NotAReference;
        generation = 0;
                
        if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out num, out var next))
            return (rr.IsCompleted, reader.Sequence.Start);
        if (IsInvalidReferencePart(num, next))
            return (true, reader.Sequence.Start);
        if (!NextTokenFinder.SkipToNextToken(ref reader))
            return (rr.IsCompleted, reader.Sequence.Start);
        if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out generation, out next))
            return (rr.IsCompleted, reader.Sequence.Start);
        if (IsInvalidReferencePart(generation, next)) 
            return (true, reader.Sequence.Start);
        if (!NextTokenFinder.SkipToNextToken(ref reader, out var operation)) 
            return (rr.IsCompleted, reader.Sequence.Start);
                
        switch ((char) operation)
        {
            case 'R':
                kind = ParseResult.FoundReference;
                return (true, reader.Position);
            case 'o':
                kind = ParseResult.FoundDefinition;
                return (true, reader.Position);
            default:
                return (true, reader.Sequence.Start);
        }
    }

    private  bool IsInvalidReferencePart(int num, byte next) =>
        num < 0 || !CharClassifier.IsWhite(next);
}