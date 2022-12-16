using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.LowLevel.Writers.PageExtraction;

public readonly partial struct DeepCopy
{
    [FromConstructor] private readonly ILowLevelDocumentBuilder creator;
    private readonly Dictionary<(int, int), PdfObject> buffer = new();

    public async ValueTask<PdfObject> Clone(PdfObject itemValue) => itemValue switch
    {
        PdfIndirectObject pio => await CloneIndirectObject(pio).CA(),
        PdfArray pa => await DeepCloneArray(pa).CA(),
        PdfStream ps => await CopyPdfStream(ps).CA(),
        PdfDictionary pd => (await CopyPdfDictionary(pd).CA()).AsDictionary(),
        _ => itemValue
    };

    private async ValueTask<PdfObject> CloneIndirectObject(PdfIndirectObject pio)
    {
        if (buffer.TryGetValue((pio.GenerationNumber, pio.ObjectNumber), out var item)) return item;
        var newPio = ReserveIndirectMapping(pio);
        ((UnknownIndirectObject)newPio).SetValue(await Clone(await pio.DirectValueAsync().CA()).CA());
        return newPio;
    }

    public PdfIndirectObject ReserveIndirectMapping(PdfIndirectObject pio)
    {
        var newPio = creator.AsIndirectReference(null);
        buffer.Add((pio.GenerationNumber, pio.ObjectNumber), newPio);
        return newPio;
    }

    public void SetIndirectMapping(PdfIndirectObject source, PdfObject target)
    {
        var mappedPio = buffer[(source.GenerationNumber, source.ObjectNumber)];
        ((UnknownIndirectObject)mappedPio).SetValue(target);
    }

    private async Task<PdfStream> CopyPdfStream(PdfStream ps) =>
        (await CopyPdfDictionary(ps).CA()).AsStream(
            await ps.StreamContentAsync(StreamFormat.DiskRepresentation).CA(), StreamFormat.DiskRepresentation);

    private async ValueTask<DictionaryBuilder> CopyPdfDictionary(PdfDictionary dict)
    {
        var ret = new DictionaryBuilder();
        foreach (var item in dict.RawItems)
        {
            ret.WithItem(item.Key, await Clone(item.Value).CA());
        }
        return ret;
    }

    private async ValueTask<PdfObject> DeepCloneArray(PdfArray pa)
    {
        var target = new PdfObject[pa.Count];
        for (int i = 0; i < pa.Count; i++)
        {
            target[i] = await Clone(pa.RawItems[i]).CA();
        }
        return new PdfArray(target);
    }
}