using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

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
}