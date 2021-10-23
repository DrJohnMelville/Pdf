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

    public static async ValueTask<PdfArray> GetProcSetsAsync<T>(this T item)
        where T:IHasPageAttributes
    {
        if (item.LowLevel.TryGetValue(KnownNames.Resources, out var dictTask) &&
            await dictTask is PdfDictionary dict)
        {
            if (dict.TryGetValue(KnownNames.ProcSet, out var pSetTask) &&
                await pSetTask is PdfArray ret) return ret;
        }

        var parent = (await GetParentAsync(item)) ??
                     throw new PdfParseException("Could not find a ProcSet enrty");
        return await parent.GetProcSetsAsync();
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