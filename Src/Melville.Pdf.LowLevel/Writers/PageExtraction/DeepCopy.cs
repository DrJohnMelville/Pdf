using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
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
    [FromConstructor] private readonly IPdfObjectRegistry creator;
    
    private readonly Dictionary<(int, int), PdfObject> buffer = new();

    /// <summary>
    /// Clone an object, making a deep copy
    /// </summary>
    /// <param name="itemValue">The item to clone</param>
    /// <returns>The clones item</returns>
    public async ValueTask<PdfObject> CloneAsync(PdfObject itemValue) => itemValue switch
    {
        PdfIndirectObject pio => await CloneIndirectObjectAsync(pio).CA(),
        PdfArray pa => await DeepCloneArrayAsync(pa).CA(),
        PdfStream ps => await CopyPdfStreamAsync(ps).CA(),
        PdfDictionary pd => (await CopyPdfDictionaryAsync(pd).CA()).AsDictionary(),
        _ => itemValue
    };

    private async ValueTask<PdfObject> CloneIndirectObjectAsync(PdfIndirectObject pio)
    {
        if (buffer.TryGetValue((pio.GenerationNumber, pio.ObjectNumber), out var item)) return item;
        var newPio = creator.AddPromisedObject();
        ReserveIndirectMapping(pio, newPio);
        ((PromisedIndirectObject)newPio).SetValue(await CloneAsync(await pio.DirectValueAsync().CA()).CA());
        return newPio;
    }

    /// <summary>
    /// Reserve an object number and associate it with the given indirect object
    /// </summary>
    /// <param name="pio">The CodeSource indirect object that maps to the reserved object</param>
    /// <param name="promise">The object mapping to the indirect object in the copy</param>
    public void ReserveIndirectMapping(PdfIndirectObject pio, PdfIndirectObject promise)
    {
        buffer.Add((pio.GenerationNumber, pio.ObjectNumber), promise);
    }
    
    private async Task<PdfStream> CopyPdfStreamAsync(PdfStream ps) =>
        (await CopyPdfDictionaryAsync(ps).CA()).AsStream(
            await ps.StreamContentAsync(StreamFormat.DiskRepresentation).CA(), StreamFormat.DiskRepresentation);

    private async ValueTask<DictionaryBuilder> CopyPdfDictionaryAsync(PdfDictionary dict)
    {
        var ret = new DictionaryBuilder();
        foreach (var item in dict.RawItems)
        {
            ret.WithItem(item.Key, await CloneAsync(item.Value).CA());
        }
        return ret;
    }

    private async ValueTask<PdfObject> DeepCloneArrayAsync(PdfArray pa)
    {
        var target = new PdfObject[pa.Count];
        for (int i = 0; i < pa.Count; i++)
        {
            target[i] = await CloneAsync(pa.RawItems[i]).CA();
        }
        return new PdfArray(target);
    }
}