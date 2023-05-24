using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class PdfDictionaryParser : IPdfObjectParser
{
    // public static readonly byte[] StandardPrefix = { (byte)'<', (byte)'<' };
    // public static readonly byte[] InlineImagePrefix = { (byte)'B', (byte)'I' };
    
    private readonly IPdfObjectParser nameParser;
    private readonly IPdfObjectParser valueParser;

    public PdfDictionaryParser(IPdfObjectParser nameParser, IPdfObjectParser valueParser)
    {
        this.nameParser = nameParser;
        this.valueParser = valueParser;
    }

    public async Task<PdfObject> ParseAsync(IParsingReader source) =>
        new PdfDictionaryConcrete(await ParseDictionaryItemsAsync(source).CA());

    public async ValueTask<Memory<KeyValuePair<PdfName, PdfObject>>> 
        ParseDictionaryItemsAsync(IParsingReader source)
    {
        var reader = await source.Reader.ReadAsync().CA();
        //This has to succeed because the prior parser looked at the prefix to get here.
        source.Reader.AdvanceTo(reader.Buffer.GetPosition(2));

        return await ParseItemRecursiveAsync(source, 0).CA();
    }

    private async ValueTask<KeyValuePair<PdfName, PdfObject>[]> ParseItemRecursiveAsync(IParsingReader source, int position)
    {
        var key = await nameParser.ParseAsync(source).CA();
        if (key == PdfTokenValues.DictionaryTerminator)
        {
            return new KeyValuePair<PdfName, PdfObject>[position];
        }

        var item = await valueParser.ParseAsync(source).CA();

        if (ShouldSkipNullValuedItem(item)) 
            return await ParseItemRecursiveAsync(source, position).CA();

        return AddItemToFinalArray(await ParseItemRecursiveAsync(source, position + 1).CA(), position, item, key);
    }

    private static KeyValuePair<PdfName, PdfObject>[] AddItemToFinalArray(
        KeyValuePair<PdfName, PdfObject>[] ret, int position, PdfObject item, PdfObject key)
    {
        CheckValueIsNotTerminator(item);
        ret[position] = KeyValuePair.Create(CheckIfKeyIsName(key), item);
        return ret;
    }

    private static bool ShouldSkipNullValuedItem(PdfObject item) => ReferenceEquals(item, PdfTokenValues.Null);
    
    private static void CheckValueIsNotTerminator(PdfObject item)
    {
        if (item == PdfTokenValues.DictionaryTerminator)
            throw new PdfParseException("Dictionary must have an even number of children.");
    }

    private static PdfName CheckIfKeyIsName(PdfObject? name) =>
        name as PdfName ?? throw new PdfParseException("Dictionary keys must be names");
}