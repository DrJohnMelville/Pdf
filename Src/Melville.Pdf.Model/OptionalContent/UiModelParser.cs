using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Parsing.SpanAndMemory;

namespace Melville.Pdf.Model.OptionalContent;

internal readonly struct UiModelParser
{
    private readonly Dictionary<PdfDictionary, OptionalGroup> groupStates;

    public UiModelParser(Dictionary<PdfDictionary, OptionalGroup> groupStates)
    {
        this.groupStates = groupStates;
    }
    public async ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ParseAsync(PdfArray? order)
    {
        if (order is null or {Count:0} ) return Array.Empty<IOptionalContentDisplayGroup>();
        var items = await order.AsDirectValuesAsync().CA();
        return await ParseMultipleOptionDisplayGroupsAsync(items).CA();
    }

    private async ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ParseMultipleOptionDisplayGroupsAsync(
        IReadOnlyList<PdfDirectObject> items)
    {
        if (items.Count == 0) return Array.Empty<IOptionalContentDisplayGroup>();
        var ret = new IOptionalContentDisplayGroup[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            ret[i] = await ParseUiModelElementAsync(items[i]).CA();
        }
        return ret;
    }

    private async ValueTask<IOptionalContentDisplayGroup> ParseUiModelElementAsync(PdfDirectObject item) =>
        item switch
        {
            var x when item.TryGet(out PdfDictionary? dict) => groupStates[dict],
            var x when item.TryGet(out PdfArray? arr) => await ParseUiModelArrayAsync(await arr.AsDirectValuesAsync().CA()).CA(),
            _ => throw new PdfParseException("Unexpected Order member in optional content group")
        };

    private async ValueTask<IOptionalContentDisplayGroup> ParseUiModelArrayAsync(IReadOnlyList<PdfDirectObject> array)
    {
        if (array.Count < 1)
            throw new PdfParseException("Empty Order Subarray in optional content group.");
        return array[0] switch
        {
            {IsString:true} str => new OptionalContentPickerTitle(str.Get<string>(),
                await ParseChildrenAsync(array).CA()),
            var x when x.TryGet(out PdfDictionary? dict) => new OptionalGroupUiView(groupStates[dict],
                await ParseChildrenAsync(array).CA()),
            _=> throw new PdfParseException("Unexpected Order member in optional content group")
        };
    }

    private ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ParseChildrenAsync(IReadOnlyList<PdfDirectObject> array) =>
        ParseMultipleOptionDisplayGroupsAsync(array.Slice(1));
}