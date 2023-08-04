using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.LowLevel.Writers.PageExtraction;

/// <summary>
/// This struct copies a pdf object, and all the objects that it transitively references
/// into a new IPdfObjectRegistry.  This is used in the ComparingReader to extract a single page from
/// a PDF file.
/// </summary>
public readonly partial struct DeepCopy
{
    /// <summary>
    /// An IPdfObjectRegistry that is the target of the copy operation
    /// </summary>
    [FromConstructor] private readonly IPdfObjectCreatorRegistry creator;
    
    private readonly Dictionary<(int, int), PdfIndirectObject> buffer = new();

    /// <summary>
    /// Clone an object, making a deep copy
    /// </summary>
    /// <param name="itemValue">The item to clone</param>
    /// <returns>The clones item</returns>
    public ValueTask<PdfIndirectObject> CloneAsync(PdfIndirectObject itemValue) => 
        itemValue.TryGetEmbeddedDirectValue(out PdfDirectObject pdv) ? 
            CloneDirectValueAsync(pdv) : 
            CloneReferenceValueAsync(itemValue);


    private ValueTask<PdfIndirectObject> CloneDirectValueAsync(PdfDirectObject value) => value switch
    {
        var x when x.TryGet(out PdfArray? arr) => DuplicateArrayAsync(arr),
        var x when x.TryGet(out PdfStream? str) => DuplicateStreamAsync(str),
        var x when x.TryGet(out PdfDictionary? dict) => DuplicateDictionaryAsync(dict),
        _ => new(value)
    };

    private async ValueTask<PdfIndirectObject> DuplicateDictionaryAsync(PdfDictionary value) =>
        (await DuplicateDictionaryBuilderAsync(value).CA()).AsDictionary();

    private async ValueTask<PdfIndirectObject> DuplicateStreamAsync(PdfStream value) => 
        (await DuplicateDictionaryBuilderAsync(value).CA())
        .AsStream(await value.StreamContentAsync(StreamFormat.DiskRepresentation).CA(), StreamFormat.DiskRepresentation);

    private async Task<DictionaryBuilder> DuplicateDictionaryBuilderAsync(PdfDictionary value)
    {
        var builder = new DictionaryBuilder();
        foreach (var pair in value.RawItems)
        {
            builder.WithItem(pair.Key, await CloneAsync(pair.Value).CA());
        }

        return builder;
    }

    private async ValueTask<PdfIndirectObject> DuplicateArrayAsync(PdfArray value)
    {
        var ret = new PdfIndirectObject[value.Count];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = await CloneAsync(value.RawItems[i]).CA();
        }

        return new PdfArray(ret);
    }

    private async ValueTask<PdfIndirectObject> CloneReferenceValueAsync(PdfIndirectObject itemValue)
    {
        var pair = itemValue.GetObjectReference();
        if (buffer.TryGetValue(pair, out var ret)) return ret;

        var clonedDirectRef = await CloneDirectValueAsync(await itemValue.LoadValueAsync().CA()).CA();
        var newIndirectRef = creator.Add(await clonedDirectRef.LoadValueAsync().CA());
        buffer[pair] = newIndirectRef;
        return newIndirectRef;
    }

    public void ReserveIndirectMapping(int objNum, int gen, PdfIndirectObject targetPromise) => 
        buffer[(objNum, gen)] = targetPromise;
}