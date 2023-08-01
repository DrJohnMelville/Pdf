using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

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
    
    private readonly Dictionary<(int, int), PdfIndirectValue> buffer = new();

    /// <summary>
    /// Clone an object, making a deep copy
    /// </summary>
    /// <param name="itemValue">The item to clone</param>
    /// <returns>The clones item</returns>
    public ValueTask<PdfIndirectValue> CloneAsync(PdfIndirectValue itemValue) => 
        itemValue.TryGetEmbeddedDirectValue(out PdfDirectValue pdv) ? 
            CloneDirectValueAsync(pdv) : 
            CloneReferenceValueAsync(itemValue);


    private ValueTask<PdfIndirectValue> CloneDirectValueAsync(PdfDirectValue value) => value switch
    {
        var x when x.TryGet(out PdfValueArray? arr) => DuplicateArrayAsync(arr),
        var x when x.TryGet(out PdfValueStream? str) => DuplicateStreamAsync(str),
        var x when x.TryGet(out PdfValueDictionary? dict) => DuplicateDictionaryAsync(dict),
        _ => new(value)
    };

    private async ValueTask<PdfIndirectValue> DuplicateDictionaryAsync(PdfValueDictionary value) =>
        (await DuplicateDictionaryBuilderAsync(value).CA()).AsDictionary();

    private async ValueTask<PdfIndirectValue> DuplicateStreamAsync(PdfValueStream value) => 
        (await DuplicateDictionaryBuilderAsync(value).CA())
        .AsStream(await value.StreamContentAsync(StreamFormat.DiskRepresentation).CA());

    private async Task<ValueDictionaryBuilder> DuplicateDictionaryBuilderAsync(PdfValueDictionary value)
    {
        var builder = new ValueDictionaryBuilder();
        foreach (var pair in value.RawItems)
        {
            builder.WithItem(pair.Key, await CloneAsync(pair.Value).CA());
        }

        return builder;
    }

    private async ValueTask<PdfIndirectValue> DuplicateArrayAsync(PdfValueArray value)
    {
        var ret = new PdfIndirectValue[value.Count];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = await CloneAsync(value.RawItems[i]).CA();
        }

        return new PdfValueArray(ret);
    }

    private async ValueTask<PdfIndirectValue> CloneReferenceValueAsync(PdfIndirectValue itemValue)
    {
        var pair = itemValue.GetObjectReference();
        if (buffer.TryGetValue(pair, out var ret)) return ret;

        var clonedDirectRef = await CloneDirectValueAsync(await itemValue.LoadValueAsync().CA()).CA();
        var newIndirectRef = creator.Add(await clonedDirectRef.LoadValueAsync().CA());
        buffer[pair] = newIndirectRef;
        return newIndirectRef;
    }

    public void ReserveIndirectMapping(int objNum, int gen, PdfIndirectValue targetPromise) => 
        buffer[(objNum, gen)] = targetPromise;
}