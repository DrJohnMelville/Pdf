using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

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
        var ocgs = await (await oCProperties.GetAsync<PdfArray>(KnownNames.OCGs).CA()).AsAsync<PdfDictionary>().CA();
        foreach (var ocg in ocgs)
        {
            ocDict[ocg] = await ParseOptionalGroupAsync(ocg).CA();
        }

        var configs = new List<OptionalContentConfiguration>();
        if (oCProperties.TryGetValue(KnownNames.D, out var dTask) && 
            (await dTask.CA()) is PdfDictionary occ)
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
        var intent = await (await ocg.GetOrNullAsync(KnownNames.Intent).CA()).ObjectAsResolvedListAsync<PdfName>().CA();
        return new OptionalGroup(
            (await ocg.GetOrDefaultAsync(KnownNames.Name, PdfString.Empty).CA()).AsTextString())
        {
            Visible = intent.Contains(KnownNames.View)
        };
    }

    private  async ValueTask<OptionalContentConfiguration> ParseOptionalContentConfigurationAsync(
        PdfDictionary occ)
    {
        var name = await occ.GetOrDefaultAsync(KnownNames.Name, PdfString.Empty).CA();
        var creator = await occ.GetOrDefaultAsync(KnownNames.Creator, PdfString.Empty).CA();
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

    private async Task<IReadOnlyList<OptionalContentExclusionGroup>> ParseRadioButtonGroupsAsync(PdfArray rbGroups)
    {
        var dicts = await rbGroups.AsAsync<PdfArray>().CA();
        var ret = new OptionalContentExclusionGroup[dicts.Length];
        for (int i = 0; i < ret.Length; i++)
        {
            var groupDicts = await dicts[i].AsAsync<PdfDictionary>().CA();
            var groupItems = new OptionalGroup[groupDicts.Length];
            for (int j = 0; j < groupItems.Length; j++)
            {
                groupItems[j] = ocDict[groupDicts[j]];
            }

            ret[i] = new OptionalContentExclusionGroup(groupItems);
        }

        return ret;
    }

    private bool? ParseBaseState(PdfObject baseStateVal)
    {
        bool? baseState = baseStateVal.GetHashCode() switch
        {
            KnownNameKeys.ON => true,
            KnownNameKeys.OFF => false,
            _ => null
        };
        return baseState;
    }

    private async ValueTask<PdfDictionary[]> ParseOnOffArrayAsync(PdfDictionary occ, PdfName dictName)
    {
        return await (await occ.GetOrDefaultAsync(dictName, PdfArray.Empty).CA())
            .AsAsync<PdfDictionary>().CA();
    }
}