using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.PdfStreamHolders;

namespace Melville.Pdf.LowLevel.Parsing.NameParsing
{
    public class PdfDictionaryParser : IPdfObjectParser
    {
        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            var reader = await source.ReadAsync();
            //This has to succeed because the prior parser looked at the prefix to get here.
            source.AdvanceTo(reader.Buffer.GetPosition(2));
            var dictionary = new Dictionary<PdfName, PdfObject>();
            while (true)
            {
                var name = await source.RootParser.ParseAsync(source);
                if (name == PdfEmptyConstants.DictionaryTerminator)
                {
                    //TODO: See how much the trim helps in memory and costs in speed.
                    dictionary.TrimExcess();
                    return new PdfDictionary(dictionary);
                }
                if (name is not PdfName typedAsName)
                    throw new PdfParseException("Dictionary keys must be names");
                var item = await source.RootParser.ParseAsync(source);
                if (item == PdfEmptyConstants.Null) continue;
                if (item == PdfEmptyConstants.DictionaryTerminator)
                    throw new PdfParseException("Dictionary must have an even number of children.");
                dictionary[typedAsName] = item;
                dictionary.TrimExcess();
            }
        }
    }
}