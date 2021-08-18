using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public interface IIndirectObjectResolver
    {
        IReadOnlyDictionary<(int, int), PdfIndirectReference> GetObjects();
        PdfIndirectReference FindIndirect(int number, int generation);
        void AddLocationHint(int number, int generation, Func<ValueTask<PdfObject>> valueAccessor);
        Task<long> FreeListHead();
    }

    public class IndirectObjectParser : IPdfObjectParser
    {
        private readonly IPdfObjectParser fallbackNumberParser;

        public IndirectObjectParser(IPdfObjectParser fallbackNumberParser)
        {
            this.fallbackNumberParser = fallbackNumberParser;
        }

        public async Task<PdfObject> ParseAsync(IParsingReader source)
        {
            ParseResult kind;
            PdfIndirectReference? reference;
            do{}while(source.ShouldContinue(ParseReference(await source.ReadAsync(), 
                source.IndirectResolver, out kind, out reference!)));

            switch (kind)
            {
                case ParseResult.FoundReference:
                    return reference;
                
                case ParseResult.FoundDefinition:
                    do { } while (source.ShouldContinue(SkipToObjectBeginning(await source.ReadAsync())));
                    var target = await source.RootObjectParser.ParseAsync(source);
                    await NextTokenFinder.SkipToNextToken(source);
                    do { } while (source.ShouldContinue(SkipEndObj(await source.ReadAsync())));
                    return target;
                
                case ParseResult.NotAReference:
                    return await fallbackNumberParser.ParseAsync(source);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private (bool Success, SequencePosition Position) SkipToObjectBeginning(ReadResult source)
        {
            var reader = new SequenceReader<byte>(source.Buffer);
            return (reader.TryAdvance(2) && NextTokenFinder.SkipToNextToken(ref reader), reader.Position);

        }
        private (bool Success, SequencePosition Position) SkipEndObj(ReadResult source)
        {
            if (source.Buffer.Length < 6) return (false, source.Buffer.Start);
            return (true, source.Buffer.GetPosition(6));
        }

        private enum ParseResult
        {
            FoundReference,
            FoundDefinition,
            NotAReference,
        }
        
        
        private (bool, SequencePosition) ParseReference(ReadResult rr,
            IIndirectObjectResolver resolver, out ParseResult kind, out PdfIndirectReference? reference)
        {
            var reader = new SequenceReader<byte>(rr.Buffer);
            reference = null;
            kind = ParseResult.NotAReference;
                
            if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out int num, out var next))
                return (false, reader.Position);
            if (IsInvalidReferencePart(num, next))
                return (true, reader.Sequence.Start);
            if (!NextTokenFinder.SkipToNextToken(ref reader))
                return (false, reader.Position);
            if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out int generation, out next))
                return (false, reader.Position);
            if (IsInvalidReferencePart(generation, next)) 
                return (true, reader.Sequence.Start);
            if (!NextTokenFinder.SkipToNextToken(ref reader, out var operation)) 
                return (false, reader.Position);
                
            switch ((char) operation)
            {
                case 'R':
                    kind = ParseResult.FoundReference;
                    reference = resolver.FindIndirect(num, generation);
                    return (true, reader.Position);
                case 'o':
                    kind = ParseResult.FoundDefinition;
                    reference = resolver.FindIndirect(num, generation);
                    return (true, reader.Position);
                default:
                    return (true, reader.Sequence.Start);
            }
        }

        private  bool IsInvalidReferencePart(int num, byte next) =>
            num < 0 || CharClassifier.Classify(next) != CharacterClass.White;
    }
}