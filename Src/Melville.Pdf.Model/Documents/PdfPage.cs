using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Documents;

public interface IHasPageAttributes
{
    PdfDictionary LowLevel { get; }
}

public static class PdfPageAttributes
{
    //this odd generic construction gives us poymorphism over the structs without boxing them
    public static async ValueTask<PdfPageParent?> GetParentAsync<T>(this T item) 
        where T : IHasPageAttributes =>
        item.LowLevel.TryGetValue(KnownNames.Parent, out var parentTask) && 
        await parentTask is PdfDictionary dict ? 
            new PdfPageParent(dict): 
            null;

    private static async ValueTask<PdfObject> GetResourceDictionaryAsync<T>(
        this T item, PdfName name) where T : IHasPageAttributes =>
        item.LowLevel.TryGetValue(KnownNames.Resources, out var resTask) &&
        await resTask is PdfDictionary dict &&
        dict.TryGetValue(name, out var itemTask) &&
        await itemTask is { } ret
            ? ret
            : PdfTokenValues.Null;

    public static async ValueTask<PdfArray> GetProcSetsAsync<T>(this T item)
        where T:IHasPageAttributes
    {
        if (await GetResourceDictionaryAsync(item, KnownNames.ProcSet) is
            PdfArray arr) return arr;

        var parent = (await GetParentAsync(item)) ??
                     throw new PdfParseException("Could not find a ProcSet enrty");
        return await parent.GetProcSetsAsync();
    }

    public static ValueTask<PdfObject> GetXrefObjectAsync<T>(this T item, PdfName name)
        where T : IHasPageAttributes => 
        TwoLevelResourceDictionaryAccess(item, KnownNames.XObject, name);

    private static async ValueTask<PdfObject> TwoLevelResourceDictionaryAccess<T>(
        T item, PdfName subDictionaryName, PdfName name) where T : IHasPageAttributes
    {
        var resourceDict = await GetResourceDictionaryAsync(item, subDictionaryName);
        if (resourceDict is PdfDictionary resDict && 
            resDict.TryGetValue(name, out var itemTask) &&
            await itemTask is {} ret && ret != PdfTokenValues.Null) return ret;
        return (await GetParentAsync(item)) is {} parent ?
                await TwoLevelResourceDictionaryAccess(parent, subDictionaryName, name):
                PdfTokenValues.Null;

    }
}
public readonly struct PdfPageParent: IHasPageAttributes
{
    public PdfDictionary LowLevel { get; }

    public PdfPageParent(PdfDictionary lowLevel)
    {
        LowLevel = lowLevel;
    }
}
public readonly struct PdfPage: IHasPageAttributes
{
    public PdfDictionary LowLevel { get; }

    public PdfPage(PdfDictionary lowLevel)
    {
        LowLevel = lowLevel;
    }

    public async ValueTask<PdfTime?> LastModifiedAsync()
    {
        return LowLevel.TryGetValue(KnownNames.LastModified, out var task) &&
               await task is PdfString str
            ? str.AsPdfTime()
            : null;
    }
}