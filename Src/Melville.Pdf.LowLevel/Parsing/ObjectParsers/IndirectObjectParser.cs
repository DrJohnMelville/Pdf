using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
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

    public static class IndirectObjectResolverOperations
    {
        public static void RegistedDeletedBlock(
            this IIndirectObjectResolver resolver, int number, int next, int generation) =>
            resolver.AddLocationHint(number, generation,
                () => new ValueTask<PdfObject>(new PdfFreeListObject(next)));
        public static void RegistedNullObject(
            this IIndirectObjectResolver resolver, int number, int next, int generation) =>
            resolver.AddLocationHint(number, generation,
                () => new ValueTask<PdfObject>(PdfTokenValues.Null));

        public static void RegisterIndirectBlock(
            this ParsingFileOwner owner, int number, long generation, long offset)
        {
            owner.IndirectResolver.AddLocationHint(number, (int)generation,
                async () =>
                {
                    using var rentedReader = await owner.RentReader(offset);
                    return await rentedReader.RootObjectParser.ParseAsync(rentedReader);
                });
        }
        public static void RegisterObjectStreamBlock(
            this ParsingFileOwner owner, int number, long referredStream, long referredOrdinal)
        {
            owner.IndirectResolver.AddLocationHint(number, 0,
                async () =>
                {
                    var stream = (PdfStream)await owner.IndirectResolver
                            .FindIndirect((int)referredStream, 0).DirectValue();
                    return await LoadObjectStream(owner, stream, number);
                });
        }
        
        public static async ValueTask<PdfObject> LoadObjectStream(
            ParsingFileOwner owner, PdfStream source, int objectNumber)
        {
            PdfObject ret = PdfTokenValues.Null;
            await using var data = await source.GetDecodedStreamAsync();
            var reader = owner.ParsingReaderForStream(data, 0);
            var objectLocations = await ObjectStreamOperations.GetIncludedObjectNumbers(
                source, reader.AsPipeReader());
            var first = (await source.GetAsync<PdfNumber>(KnownNames.First)).IntValue;
            foreach (var location in objectLocations)
            {
                 await reader.AdvanceToPositionAsync(first + location.Offset);
                 var obj = await owner.RootObjectParser.ParseAsync(reader);
                 if (objectNumber == location.ObjectNumber)
                     ret = obj;
                AcceptObject(owner.IndirectResolver,location.ObjectNumber,obj);
            }

            return ret;
        }
        
        public static void AcceptObject(IIndirectObjectResolver resolver,
            int objectNumber, PdfObject pdfObject) =>
            ((IMultableIndirectObject)resolver.FindIndirect(objectNumber, 0).Target).SetValue(pdfObject);


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
                return (rr.IsCompleted, reader.Sequence.Start);
            if (IsInvalidReferencePart(num, next))
                return (true, reader.Sequence.Start);
            if (!NextTokenFinder.SkipToNextToken(ref reader))
                return (rr.IsCompleted, reader.Sequence.Start);
            if (!WholeNumberParser.TryParsePositiveWholeNumber(ref reader, out int generation, out next))
                return (rr.IsCompleted, reader.Sequence.Start);
            if (IsInvalidReferencePart(generation, next)) 
                return (true, reader.Sequence.Start);
            if (!NextTokenFinder.SkipToNextToken(ref reader, out var operation)) 
                return (rr.IsCompleted, reader.Sequence.Start);
                
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