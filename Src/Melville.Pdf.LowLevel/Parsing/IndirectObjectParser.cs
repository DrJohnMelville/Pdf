using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.PdfStreamHolders;

namespace Melville.Pdf.LowLevel.Parsing
{
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
                switch (ParseReference(ref input, out var endPos, out var reference))
                {
                    case ParseResult.FoundReference:
                        source.AdvanceTo(endPos);
                        return reference!;
                    case ParseResult.FoundDefinition:
                        source.AdvanceTo(endPos);
                        ((ICanSetIndirectTarget)reference!.Target).SetValue(
                            await source.RootParser.ParseAsync(source));
                        await NextTokenFinder.SkipToNextToken(source);
                        while (true)
                        {
                            var chars = await source.ReadAsync();
                            if (chars.Buffer.Length >= 6)
                            {
                                source.AdvanceTo(chars.Buffer.GetPosition(6));
                                break;
                            }
                            source.NeedMoreInputToAdvance();
                        }
                        return reference.Target;
                        break;
                    case ParseResult.NeedMoreChars:
                        source.NeedMoreInputToAdvance();
                        break;
                    case ParseResult.NotAReference:
                        return await fallbackNumberParser.ParseAsync(source);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private enum ParseResult
        {
            FoundReference,
            FoundDefinition,
            NeedMoreChars,
            NotAReference,
        }

        private ParseResult ParseReference(
            ref ReadOnlySequence<byte> text, out SequencePosition lastConsumed, 
            out PdfIndirectReference? reference)
        {
            lastConsumed = text.GetPosition(0);
            reference = null;
            var reader = new SequenceReader<byte>(text);
            if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out var num, out var next)) 
                return ParseResult.NeedMoreChars;
            if (IsInvalidReferencePart(num, next)) return ParseResult.NotAReference;
            
            if (!NextTokenFinder.SkipToNextToken(ref reader)) return ParseResult.NeedMoreChars;
            if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out var generation, out next))
                return ParseResult.NeedMoreChars;
            if (IsInvalidReferencePart(generation, next)) return ParseResult.NotAReference;
            
            if (!NextTokenFinder.SkipToNextToken(ref reader)) return ParseResult.NeedMoreChars;
            if (!reader.TryRead(out var operation)) return ParseResult.NeedMoreChars;
            switch ((char)operation)
            {
                case 'R':
                    lastConsumed = reader.Position;
                    reference = LookupReference(num, generation);
                    return ParseResult.FoundReference;
                case 'o':
                    if (!TryAdvanceToReferenceDefinition(ref reader)) return ParseResult.NeedMoreChars;
                    lastConsumed = reader.Position;
                    reference = LookupReference(num, generation);
                    return ParseResult.FoundDefinition;
                default:
                    return ParseResult.NotAReference;
            }
        }

        private static PdfIndirectReference LookupReference(int num, int generation)
        {
            return new PdfIndirectReference(new PdfIndirectObject(num, generation));
        }

        private static bool IsInvalidReferencePart(int num, byte next) => 
            num < 0 || CharClassifier.Classify(next) != CharacterClass.White;

        private static bool TryAdvanceToReferenceDefinition(ref SequenceReader<byte> reader) => 
            reader.TryAdvance(2) && NextTokenFinder.SkipToNextToken(ref reader);
    }
}