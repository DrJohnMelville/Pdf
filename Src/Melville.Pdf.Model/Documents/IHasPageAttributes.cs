using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;

namespace Melville.Pdf.Model.Documents;

public interface IHasPageAttributes
{
    PdfDictionary LowLevel { get; }
}

public static partial class PdfPageAttributes
{
    //this odd generic construction gives us poymorphism over the structs without boxing them
    public static async ValueTask<PdfPageParent?> GetParentAsync<T>(this T item)
        where T : IHasPageAttributes =>
        item.LowLevel.TryGetValue(KnownNames.Parent, out var parentTask) &&
        await parentTask is PdfDictionary dict
            ? new PdfPageParent(dict)
            : null;

    private static async ValueTask<PdfObject> GetResourceDictionaryAsync<T>(
        this T item, PdfName name) where T : IHasPageAttributes =>
        item.LowLevel.TryGetValue(KnownNames.Resources, out var resTask) &&
        await resTask is PdfDictionary dict &&
        dict.TryGetValue(name, out var itemTask) &&
        await itemTask is { } ret
            ? ret
            : PdfTokenValues.Null;

    private static async IAsyncEnumerable<PdfObject> InheritedPageProperties(
        PdfDictionary item, PdfName name)
    {
        var dict = item;
        while (true)
        {
            if (dict.TryGetValue(name, out var retTask) && await retTask is { } ret &&
                ret != PdfTokenValues.Null) yield return ret;
            var parent = await dict.GetOrNullAsync(KnownNames.Parent);
            if (parent is not PdfDictionary parentDict) yield break;
            dict = parentDict;
        }
    }

    private static IAsyncEnumerable<PdfObject> InheritedResourceItem(PdfDictionary item, PdfName name) =>
        InheritedPageProperties(item, KnownNames.Resources)
            .OfType<PdfDictionary>()
            .SelectAwait(i => i.GetOrNullAsync(name))
            .Where(i => i != PdfTokenValues.Null);

    public static async ValueTask<PdfArray?> GetProcSetsAsync<T>(this T item)
        where T : IHasPageAttributes
    {
        return await InheritedResourceItem(item.LowLevel, KnownNames.ProcSet).OfType<PdfArray>()
            .FirstOrDefaultAsync();
    }
    
    public static ValueTask<PdfObject?> GetResourceObject<T>(
        this T item, ResourceTypeName resourceType, PdfName name) where T : IHasPageAttributes =>
        TwoLevelResourceDictionaryAccess(item.LowLevel, resourceType, name);

    private static ValueTask<PdfObject?> TwoLevelResourceDictionaryAccess(
        PdfDictionary item, PdfName subDictionaryName, PdfName name) =>
        InheritedResourceItem(item, subDictionaryName)
            .OfType<PdfDictionary>()
            .SelectAwait(i => i.GetOrNullAsync(name))
            .Where(i => i != PdfTokenValues.Null)
            .DefaultIfEmpty(PdfTokenValues.Null)
            .FirstOrDefaultAsync();

    private static ValueTask<PdfRect?> GetSingleBoxAsync(PdfDictionary item, PdfName name) =>
        InheritedPageProperties(item, name)
            .OfType<PdfArray>()
            .SelectAwait(PdfRect.CreateAsync)
            .Select(i => new PdfRect?(i))
            .DefaultIfEmpty(new PdfRect?())
            .FirstOrDefaultAsync();

    // In the PDF Spec Version 7.7.3.3 Table 30, only mediabox and cropbpx are inheritable
    // for symmetry we implement them all is inheritable.  This is harmless, because a writer
    // would have no reason to put a noninheritable property anywhere but in the page node.
    public static async ValueTask<PdfRect?> GetBoxAsync<T>(this T item, BoxName boxType)
        where T : IHasPageAttributes =>
        await GetSingleBoxAsync(item.LowLevel, boxType) ??
        await GetBoxOrDefaultAsync(item, FallbackBox(boxType));
    
    // Standard 7.7.3.3 states that media box is required, however Adobe reader parses files without mediaboxes
    // without complaining -- so we just default to letter size peper.
    private static ValueTask<PdfRect?> GetBoxOrDefaultAsync<T>(T item, BoxName? boxType)
        where T : IHasPageAttributes =>
        boxType.HasValue ? item.GetBoxAsync(boxType.Value): new(UsLetterSizedBox<T>());

    private static PdfRect UsLetterSizedBox<T>() where T : IHasPageAttributes => new(0, 0, 612, 792);

    private static BoxName? FallbackBox(BoxName boxType)
    {
        if ((PdfName)boxType == KnownNames.MediaBox) return null;
        if ((PdfName)boxType == KnownNames.CropBox) return BoxName.MediaBox;
        return BoxName.CropBox;
    }
}