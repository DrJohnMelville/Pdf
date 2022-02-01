using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

public class IndirectObjectResolver : IIndirectObjectResolver
{
    private readonly Dictionary<(int, int), PdfIndirectReference> index = new();

    public IReadOnlyDictionary<(int, int), PdfIndirectReference> GetObjects() =>
        index;

    public PdfIndirectReference FindIndirect(int objectNumber, int generation)
    {
        if (index.TryGetValue((objectNumber, generation), out var existingReference)) 
            return existingReference;
        var ret = new PdfIndirectReference(new PdfIndirectObject(objectNumber,
            generation, PdfTokenValues.Null));
        index.Add((objectNumber, generation), ret);
        return ret;
    }
        
    public void AddLocationHint(int number, int generation, Func<ValueTask<PdfObject>> valueAccessor)
    {
        var item = (IMultableIndirectObject)(FindIndirect(number, generation).Target);
        if (item.HasRegisteredAccessor()) return;
        item.SetValue(valueAccessor);
    }

    public async Task<long> FreeListHead()
    {
        return (index.TryGetValue((0, 65535), out var iRef) &&
                (await iRef.DirectValueAsync().CA()) is PdfFreeListObject flo)
            ? flo.NextItem
            : 0;
    }
}