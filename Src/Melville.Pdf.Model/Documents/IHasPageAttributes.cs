using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;

namespace Melville.Pdf.Model.Documents;

internal interface IHasPageAttributes
{
    PdfDictionary LowLevel { get; }
    ValueTask<Stream> GetContentBytesAsync();
    ValueTask<IHasPageAttributes?> GetParentAsync();

}

internal static class PdfPageAttributesOperations
{
    private static async IAsyncEnumerable<PdfObject> InheritedPagePropertiesAsync(IHasPageAttributes item, PdfName name)
    {
        var dict = item;
        while (dict != null)
        {
            if (dict.LowLevel.TryGetValue(name, out var retTask) && await retTask.CA() is { } ret &&
                ret != PdfTokenValues.Null) yield return ret;
            dict = await dict.GetParentAsync().CA();
        }
    }

    private static IAsyncEnumerable<PdfObject> InheritedResourceItemAsync(IHasPageAttributes item, PdfName name) =>
        InheritedPagePropertiesAsync(item, KnownNames.Resources)
            .OfType<PdfDictionary>()
            .SelectAwait(i => i.GetOrNullAsync(name))
            .Where(i => i != PdfTokenValues.Null);

    public static async ValueTask<PdfArray?> GetProcSetsAsync<T>(this T item)
        where T : IHasPageAttributes
    {
        return await InheritedResourceItemAsync(item, KnownNames.ProcSet).OfType<PdfArray>()
            .FirstOrDefaultAsync().CA();
    }
    
    public static ValueTask<PdfObject?> GetResourceAsync(
        this IHasPageAttributes item, ResourceTypeName resourceType, PdfName name) =>
        TwoLevelResourceDictionaryAccessAsync(item, resourceType, name);

    private static ValueTask<PdfObject?> TwoLevelResourceDictionaryAccessAsync(
        IHasPageAttributes item, PdfName subDictionaryName, PdfName name) =>
        InheritedResourceItemAsync(item, subDictionaryName)
            .OfType<PdfDictionary>()
            .SelectAwait(i => i.GetOrNullAsync(name))
            .Where(i => i != PdfTokenValues.Null)
            .DefaultIfEmpty(PdfTokenValues.Null)
            .FirstOrDefaultAsync();

    private static ValueTask<PdfRect?> GetSingleBoxAsync(IHasPageAttributes item, PdfName name) =>
        InheritedPagePropertiesAsync(item, name)
            .OfType<PdfArray>()
            .SelectAwait(PdfRect.CreateAsync)
            .Select(i => new PdfRect?(i))
            .DefaultIfEmpty(new PdfRect?())
            .FirstOrDefaultAsync();

    // In the PDF Spec Version 7.7.3.3 Table 30, only mediabox and cropbox are inheritable
    // for symmetry we implement them all boxes inheritable.  This is harmless, because a writer
    // would have no reason to put a noninheritable property anywhere but in the page node.
    public static async ValueTask<PdfRect?> GetBoxAsync(
        this IHasPageAttributes item, BoxName boxType) =>
        await GetSingleBoxAsync(item, boxType).CA() ??
        await GetBoxOrDefaultAsync(item, FallbackBox(boxType)).CA();
    
    // Standard 7.7.3.3 states that media box is required, however Adobe reader parses files without mediaboxes
    // without complaining -- so we just default to letter size paper.
    private static ValueTask<PdfRect?> GetBoxOrDefaultAsync(
        IHasPageAttributes item, BoxName? boxType) =>
        boxType.HasValue ? item.GetBoxAsync(boxType.Value): new(UsLetterSizedBox());

    private static PdfRect UsLetterSizedBox() => new(0, 0, 612, 792);

    private static BoxName? FallbackBox(BoxName boxType)
    {
        if ((PdfName)boxType == KnownNames.MediaBox) return null;
        if ((PdfName)boxType == KnownNames.CropBox) return BoxName.MediaBox;
        return BoxName.CropBox;
    }
}