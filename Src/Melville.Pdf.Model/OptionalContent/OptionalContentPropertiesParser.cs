using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.Model.OptionalContent;

internal readonly struct OptionalContentPropertiesParser
{

    private readonly Dictionary<PdfDictionary, OptionalGroup> ocDict = new();

    public OptionalContentPropertiesParser()
    {
    }

    public static ValueTask<IOptionalContentState> ParseAsync(PdfDictionary? oCProperties) =>
        oCProperties is null ? new(AllOptionalContentVisible.Instance):
            new OptionalContentPropertiesParser().ParsePropertiesAsync(oCProperties);

    private async ValueTask<IOptionalContentState> ParsePropertiesAsync(PdfDictionary oCProperties)
    {
        var ocgs = await (await oCProperties.GetAsync<PdfArray>(KnownNames.OCGs).CA()).CastAsync<PdfDictionary>().CA();
        foreach (var ocg in ocgs)
        {
            ocDict[ocg] = await ParseOptionalGroupAsync(ocg).CA();
        }

        var configs = new List<OptionalContentConfiguration>();
        if (oCProperties.TryGetValue(KnownNames.D, out var dTask) && 
            (await dTask.CA()).TryGet(out PdfDictionary? occ))
        { 
            configs.Add(await ParseOptionalContentConfigurationAsync(occ).CA());
        }

        var ret = new OptionalContentState(ocDict, configs);
        if (configs.Count > 0)
        {
            ret.ConfigureWith(configs[0]);
        }
        return ret;
    }

    private static async Task<OptionalGroup> ParseOptionalGroupAsync(PdfDictionary ocg)
    {
        var intent = (await ocg.GetOrNullAsync(KnownNames.Intent).CA()).ObjectAsUnresolvedList();
        return new OptionalGroup(
            ocg.TryGetValue(KnownNames.Name, out var nameTask) ?
                (await nameTask.CA()).DecodedString():"No Name")
        {
            Visible = await FindNameAsync(intent, KnownNames.View).CA()
        };
    }

    private static async ValueTask<bool> FindNameAsync(IEnumerable<PdfIndirectObject> list, PdfDirectObject searchedFor)
    {
        foreach (var indirectItem in list)
        {
            var dirItem = await indirectItem.LoadValueAsync().CA();
            if (dirItem.Equals(searchedFor)) return true;
        }

        return false;
    }

    private  async ValueTask<OptionalContentConfiguration> ParseOptionalContentConfigurationAsync(
        PdfDictionary occ)
    {
        var name = await occ.GetOrDefaultAsync(
            KnownNames.Name, PdfDirectObject.EmptyString).CA();
        var creator = await occ.GetOrDefaultAsync(
            KnownNames.Creator, PdfDirectObject.EmptyString).CA();
        var baseState = ParseBaseState(await occ.GetOrNullAsync(KnownNames.BaseState).CA());
        var onArr = await ParseOnOffArrayAsync(occ, KnownNames.ON).CA();
        var offArr = await ParseOnOffArrayAsync(occ, KnownNames.OFF).CA();
        var order = await occ.GetOrDefaultAsync(KnownNames.Order, PdfArray.Empty).CA();

        var rbGroups = await ParseRadioButtonGroupsAsync(
            await occ.GetOrDefaultAsync(KnownNames.RBGroups, PdfArray.Empty).CA()).CA();

        return new OptionalContentConfiguration(
            name.AsTextString(), 
            creator.AsTextString(), 
            baseState, onArr, offArr, order, rbGroups);
    }

    private async Task<IReadOnlyList<OptionalContentExclusionGroup>> 
        ParseRadioButtonGroupsAsync(PdfArray rbGroups)
    {
        var dicts = await rbGroups.CastAsync<PdfArray>().CA();
        var ret = new OptionalContentExclusionGroup[dicts.Count];
        for (int i = 0; i < ret.Length; i++)
        {
            var groupDicts = await dicts[i].CastAsync<PdfDictionary>().CA();
            var groupItems = new OptionalGroup[groupDicts.Count];
            for (int j = 0; j < groupItems.Length; j++)
            {
                groupItems[j] = ocDict[groupDicts[j]];
            }

            ret[i] = new OptionalContentExclusionGroup(groupItems);
        }

        return ret;
    }

    private bool? ParseBaseState(PdfDirectObject baseStateVal)
    {
        bool? baseState = baseStateVal switch
        {
            var x when x.Equals(KnownNames.ON) => true,
            var x when x.Equals(KnownNames.OFF) => false,
            _ => null
        };
        return baseState;
    }

    private async Task<IReadOnlyList<PdfDictionary>> ParseOnOffArrayAsync(
        PdfDictionary occ, PdfDirectObject dictName)
    {
        return await (await occ.GetOrDefaultAsync(dictName, PdfArray.Empty).CA())
            .CastAsync<PdfDictionary>().CA();
    }
}