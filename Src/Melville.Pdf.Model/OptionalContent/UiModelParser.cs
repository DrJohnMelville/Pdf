using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

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
        var items = await order.AsDirectValues().CA();
        return await ParseMultipleOptionDisplayGroupsAsync(new ReadOnlyMemory<PdfDirectObject>(items)).CA();
    }

    private async ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ParseMultipleOptionDisplayGroupsAsync(
        ReadOnlyMemory<PdfDirectObject> items)
    {
        if (items.Length == 0) return Array.Empty<IOptionalContentDisplayGroup>();
        var ret = new IOptionalContentDisplayGroup[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            ret[i] = await ParseUiModelElementAsync(items.Span[i]).CA();
        }
        return ret;
    }

    private async ValueTask<IOptionalContentDisplayGroup> ParseUiModelElementAsync(PdfDirectObject item) =>
        item switch
        {
            var x when item.TryGet(out PdfDictionary dict) => groupStates[dict],
            var x when item.TryGet(out PdfArray arr) => await ParseUiModelArrayAsync(await arr.AsDirectValues().CA()).CA(),
            _ => throw new PdfParseException("Unexpected Order member in optional content group")
        };

    private async ValueTask<IOptionalContentDisplayGroup> ParseUiModelArrayAsync(PdfDirectObject[] array)
    {
        if (array.Length< 1)
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

    private ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ParseChildrenAsync(PdfDirectObject[] array) =>
        ParseMultipleOptionDisplayGroupsAsync(new ReadOnlyMemory<PdfDirectObject>(array).Slice(1));
}