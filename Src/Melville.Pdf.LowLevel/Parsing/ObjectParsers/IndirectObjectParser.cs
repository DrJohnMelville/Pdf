using System;
using System.Buffers;
using System.Diagnostics.Tracing;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public interface IIndirectObjectResolver
    {
        PdfIndirectReference FindIndirect(int number, int generation);
    }

    public class IndirectObjectParser : IPdfObjectParser
    {
        private readonly IPdfObjectParser fallbackNumberParser;

        public IndirectObjectParser(IPdfObjectParser fallbackNumberParser)
        {
            this.fallbackNumberParser = fallbackNumberParser;
        }

        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            while (true)
            {
                var input = (await source.ReadAsync()).Buffer;
                var (result,endPos) = ParseReference(
                    ref input, source.IndirectResolver, out var reference);
                switch (result)
                {
                    case ParseResult.FoundReference:
                        source.AdvanceTo(endPos);
                        return reference!;
                    case ParseResult.FoundDefinition:
                        source.AdvanceTo(endPos);
                        ((ICanSetIndirectTarget) reference!.Target).SetValue(
                            await source.RootObjectParser.ParseAsync(source));
                        await NextTokenFinder.SkipToNextToken(source);
                        do { } while (source.ShouldContinue(SkipEndObj(await source.ReadAsync())));

                        return reference.Target;
                    case ParseResult.NeedMoreChars:
                        source.NeedMoreInputToAdvance();
                        break;
                    case ParseResult.NotAReference:
                        source.AbandonCurrentBuffer();
                        return await fallbackNumberParser.ParseAsync(source);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private (bool Success, SequencePosition Position) SkipEndObj(ReadResult source)
        {
            if (source.Buffer.Length < 6) return (false, source.Buffer.Start);
            return (true, source.Buffer.GetPosition(6));
        }

        private (ParseResult, SequencePosition) ParseReference(
            ref ReadOnlySequence<byte> text,
            IIndirectObjectResolver resolver,
            out PdfIndirectReference? reference)
        {
            return new ParseIndirectObjectTripple(text, resolver).ParseReference(out reference);
        }

        private enum ParseResult
        {
            FoundReference,
            FoundDefinition,
            NeedMoreChars,
            NotAReference,
        }

        private ref struct ParseIndirectObjectTripple
        {
            private SequenceReader<byte> reader;
            private readonly IIndirectObjectResolver resolver;

            public ParseIndirectObjectTripple(ReadOnlySequence<byte> text, IIndirectObjectResolver resolver)
            {
                this.resolver = resolver;
                reader = new SequenceReader<byte>(text);
            }
            
            public (ParseResult, SequencePosition) ParseReference(out PdfIndirectReference? reference)
            {
                reference = null;

                if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out var num, out var next))
                    return NeedMoreChars();
                if (IsInvalidReferencePart(num, next)) return NotAReference();

                if (!NextTokenFinder.SkipToNextToken(ref reader)) return NeedMoreChars();
                if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out var generation, out next))
                    return NeedMoreChars();
                if (IsInvalidReferencePart(generation, next)) return NotAReference();

                if (!NextTokenFinder.SkipToNextToken(ref reader, out var operation)) return NeedMoreChars();
                switch ((char) operation)
                {
                    case 'R':
                        reference = LookupReference(num, generation);
                        return (ParseResult.FoundReference, reader.Position);
                    case 'o':
                        if (!TryAdvanceToReferenceDefinition()) return NeedMoreChars();
                        reference = LookupReference(num, generation);
                        return (ParseResult.FoundDefinition,reader.Position);
                    default:
                        return NotAReference();
                }
            }

            private (ParseResult NeedMoreChars, SequencePosition Position) NeedMoreChars() => 
                (ParseResult.NeedMoreChars, reader.Position);

            private (ParseResult NotAReference, SequencePosition Start) NotAReference() => 
                (ParseResult.NotAReference, reader.Sequence.Start);

            private PdfIndirectReference LookupReference(int num, int generation) => 
                resolver.FindIndirect(num, generation);

            private  bool IsInvalidReferencePart(int num, byte next) =>
                num < 0 || CharClassifier.Classify(next) != CharacterClass.White;

            private  bool TryAdvanceToReferenceDefinition() =>
                reader.TryAdvance(2) && NextTokenFinder.SkipToNextToken(ref reader);
        }
    }
}