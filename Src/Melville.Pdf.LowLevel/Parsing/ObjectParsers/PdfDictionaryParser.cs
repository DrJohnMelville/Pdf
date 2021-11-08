using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public class PdfDictionaryParser : IPdfObjectParser
{
    private readonly IPdfObjectParser nameParser;
    private readonly IPdfObjectParser valueParser;

    public PdfDictionaryParser(IPdfObjectParser nameParser, IPdfObjectParser valueParser)
    {
        this.nameParser = nameParser;
        this.valueParser = valueParser;
    }

    public async Task<PdfObject> ParseAsync(IParsingReader source) =>
        new PdfDictionary(await ParseDictionaryItemsAsync(source));

    public async Task<Dictionary<PdfName, PdfObject>> ParseDictionaryItemsAsync(IParsingReader source)
    {
        var reader = await source.Reader.ReadAsync();
        //This has to succeed because the prior parser looked at the prefix to get here.
        source.Reader.AdvanceTo(reader.Buffer.GetPosition(2));
        var dictionary = new Dictionary<PdfName, PdfObject>();
        while (true)
        {
            var key = await nameParser.ParseAsync(source);
            if (key == PdfTokenValues.DictionaryTerminator)
            {
                //TODO: See how much the trim helps in memory and costs in speed.
                dictionary.TrimExcess();

                return dictionary;
            }

            var item = await valueParser.ParseAsync(source);
            if (item == PdfTokenValues.Null) continue;
            CheckValueIsNotTerminator(item);
            dictionary[CheckIfKeyIsName(key)] = item;
        }
    }
    
    private static void CheckValueIsNotTerminator(PdfObject item)
    {
        if (item == PdfTokenValues.DictionaryTerminator)
            throw new PdfParseException("Dictionary must have an even number of children.");
    }

    private static PdfName CheckIfKeyIsName(PdfObject? name) =>
        name is PdfName typedAsName ? 
            typedAsName : 
            throw new PdfParseException("Dictionary keys must be names");
}