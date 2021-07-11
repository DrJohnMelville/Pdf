using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using PdfDictionary = Melville.Pdf.LowLevel.Model.Objects.PdfDictionary;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public class PdfDictionaryAndStreamParser : IPdfObjectParser
    {
        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            var reader = await source.ReadAsync();
            //This has to succeed because the prior parser looked at the prefix to get here.
            source.AdvanceTo(reader.Buffer.GetPosition(2));
            var dictionary = new Dictionary<PdfName, PdfObject>();
            while (true)
            {
                var name = await source.RootObjectParser.ParseAsync(source);
                if (name == PdfEmptyConstants.DictionaryTerminator)
                {
                    //TODO: See how much the trim helps in memory and costs in speed.
                    dictionary.TrimExcess();
                    while (true)
                    {
                        var data = (await source.ReadAsync()).Buffer;
                        var result = SearchForStream(ref data, out var finalPos);
                        if (result != SearchForStreamResult.NotEnoughChars)
                        {
                            source.AdvanceTo(finalPos);
                            return result == SearchForStreamResult.Dictionary
                                ? new PdfDictionary(dictionary)
                                : new PdfStream(dictionary, source.Position);
                            //notice that Stream does not advance over the stream data.
                            // On 7/10/2021 I don't think I am going to need it.
                        }
                        source.NeedMoreInputToAdvance();
                    }
                }
                if (name is not PdfName typedAsName)
                    throw new PdfParseException("Dictionary keys must be names");
                var item = await source.RootObjectParser.ParseAsync(source);
                if (item == PdfEmptyConstants.Null) continue;
                if (item == PdfEmptyConstants.DictionaryTerminator)
                    throw new PdfParseException("Dictionary must have an even number of children.");
                dictionary[typedAsName] = item;
            }
        }

        private enum SearchForStreamResult
        {
            Dictionary,
            Stream,
            NotEnoughChars
        }
        private SearchForStreamResult SearchForStream(
            ref ReadOnlySequence<byte> data, out SequencePosition finalPos)
        {
            var reader = new SequenceReader<Byte>(data);
            finalPos = reader.Position;
            if (!FindStreamSuffix(ref reader, out var datum)) 
                return SearchForStreamResult.NotEnoughChars;
            if (!IsStreamSuffix(datum)) return SearchForStreamResult.Dictionary;
            if (!SkipOverStreamSuffix(ref reader)) return SearchForStreamResult.NotEnoughChars;
            finalPos = reader.Position;
            return SearchForStreamResult.Stream;
        }

        private static bool FindStreamSuffix(ref SequenceReader<byte> reader, out byte datum) => 
            NextTokenFinder.SkipToNextToken(ref reader, out datum);

        private static bool SkipOverStreamSuffix(ref SequenceReader<byte> reader) => 
            reader.TryAdvance(5) && reader.TryAdvanceTo((byte)'\n');

        private static bool IsStreamSuffix(byte datum) => datum == (int) 's';
    }
}