using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public class PdfDictionaryParser : IPdfObjectParser
{
    public static readonly byte[] StandardPrefix = { (byte)'<', (byte)'<' };
    public static readonly byte[] InlineImagePrefix = { (byte)'B', (byte)'I' };
    
    private readonly IPdfObjectParser nameParser;
    private readonly IPdfObjectParser valueParser;
    private readonly byte[] openingToken;

    public PdfDictionaryParser(IPdfObjectParser nameParser, IPdfObjectParser valueParser, byte[] openingToken)
    {
        this.nameParser = nameParser;
        this.valueParser = valueParser;
        this.openingToken = openingToken;
    }

    public async Task<PdfObject> ParseAsync(IParsingReader source) =>
        new PdfDictionary(await ParseDictionaryItemsAsync(source).CA());

    public async ValueTask<Memory<KeyValuePair<PdfName, PdfObject>>> 
        ParseDictionaryItemsAsync(IParsingReader source)
    {
        var reader = await source.Reader.Source.ReadAsync().CA();
        //This has to succeed because the prior parser looked at the prefix to get here.
        source.Reader.Source.AdvanceTo(reader.Buffer.GetPosition(2));
        var dictionary = new DictionaryBuilder();
        while (true)
        {
            var key = await nameParser.ParseAsync(source).CA();
            if (key == PdfTokenValues.DictionaryTerminator)
            {
                return dictionary.AsArray();
            }

            var item = await valueParser.ParseAsync(source).CA();
            if (item == PdfTokenValues.Null) continue;
            CheckValueIsNotTerminator(item);
            dictionary.WithItem(CheckIfKeyIsName(key), item);
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