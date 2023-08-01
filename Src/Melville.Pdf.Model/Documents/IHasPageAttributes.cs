using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.Model.Documents;

internal interface IHasPageAttributes
{
    PdfValueDictionary LowLevel { get; }
    ValueTask<Stream> GetContentBytesAsync();
    ValueTask<IHasPageAttributes?> GetParentAsync();

}

public static class SelectFromImpl
{
    public static IEnumerable<T> SelectInner<T>(this IEnumerable<PdfDirectValue> items)
    {
        foreach (var item in items)
        {
            if (item.TryGet(out T casted)) yield return casted;
        }
    }
    public static async IAsyncEnumerable<T> SelectInnerAsync<T>(this IAsyncEnumerable<PdfDirectValue> items)
    {
        await foreach (var item in items)
        {
            if (item.TryGet(out T casted)) yield return casted;
        }
    }
}

internal static class PdfPageAttributesOperations
{
    private static async IAsyncEnumerable<PdfDirectValue> InheritedPagePropertiesAsync(IHasPageAttributes item, PdfDirectValue name)
    {
        var dict = item;
        while (dict != null)
        {
            if (await dict.LowLevel.GetOrNullAsync(name).CA() is { IsNull: false } ret) yield return ret;
            dict = await dict.GetParentAsync().CA();
        }
    }

    private static IAsyncEnumerable<PdfDirectValue> InheritedResourceItemAsync(IHasPageAttributes item, PdfDirectValue name) =>
        InheritedPagePropertiesAsync(item, KnownNames.ResourcesTName)
            .SelectInnerAsync<PdfValueDictionary>()
            .SelectAwait(i => i.GetOrNullAsync(name))
            .Where(i => !i.IsNull);

    public static async ValueTask<PdfValueArray?> GetProcSetsAsync<T>(this T item)
        where T : IHasPageAttributes =>
        await InheritedResourceItemAsync(item, KnownNames.ProcSetTName)
            .Select(i=>i.TryGet(out PdfValueArray? arr) ? arr:null)
            .OfType<PdfValueArray>()
            .FirstOrDefaultAsync().CA();

    public static ValueTask<PdfDirectValue> GetResourceAsync(
        this IHasPageAttributes item, ResourceTypeName resourceType, PdfDirectValue name) =>
        TwoLevelResourceDictionaryAccessAsync(item, resourceType, name);

    private static ValueTask<PdfDirectValue> TwoLevelResourceDictionaryAccessAsync(
        IHasPageAttributes item, PdfDirectValue subDictionaryName, PdfDirectValue name) =>
        InheritedResourceItemAsync(item, subDictionaryName)
            .Select(i=>i.TryGet(out PdfValueDictionary? dict)?dict:null)
            .OfType<PdfValueDictionary>()
            .SelectAwait(i => i.GetOrNullAsync(name))
            .Where(i => !i.IsNull)
            .DefaultIfEmpty(PdfDirectValue.CreateNull())
            .FirstOrDefaultAsync();

    private static ValueTask<PdfRect?> GetSingleBoxAsync(IHasPageAttributes item, PdfDirectValue name) =>
        InheritedPagePropertiesAsync(item, name)
            .Select(i=>i.TryGet(out PdfValueArray? arr)?arr:null)
            .OfType<PdfValueArray>()
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
        if (((PdfDirectValue)boxType).Equals(KnownNames.MediaBoxTName)) return null;
        if (((PdfDirectValue)boxType).Equals(KnownNames.CropBoxTName)) return BoxName.MediaBox;
        return BoxName.CropBox;
    }
}