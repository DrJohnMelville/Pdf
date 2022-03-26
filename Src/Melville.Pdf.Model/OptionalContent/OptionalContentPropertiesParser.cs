using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public static class OptionalContentPropertiesParser
{
    public static ValueTask<IOptionalContentState> ParseAsync(PdfDictionary? oCProperties) =>
        oCProperties is null ? new(AllOptionalContentVisible.Instance):
            ParseProperties(oCProperties);

    private static async ValueTask<IOptionalContentState> ParseProperties(PdfDictionary oCProperties)
    {
        var ocgs = await (await oCProperties.GetAsync<PdfArray>(KnownNames.OCGs).CA()).AsAsync<PdfDictionary>().CA();
        var ocgDict = ocgs.ToDictionary(i => i, i => true);

        var configs = new List<OptionalContentConfiguration>();
        OptionalContentConfiguration? defaultConfig = null;
        if (oCProperties.TryGetValue(KnownNames.D, out var dTask) && 
            (await dTask.CA()) is PdfDictionary occ)
        {
            defaultConfig = await ParseOptionalContentConfiguration(occ).CA();
            configs.Add(defaultConfig);
        }

        var ret = new OptionalContentState(ocgDict, configs);
        if (defaultConfig != null)
        {
            ret.ConfigureWith(defaultConfig);
        }
        return ret;
    }

    private static async ValueTask<OptionalContentConfiguration> ParseOptionalContentConfiguration(
        PdfDictionary occ)
    {
        var name = await occ.GetOrDefaultAsync(KnownNames.Name, PdfString.Empty).CA();
        var creator = await occ.GetOrDefaultAsync(KnownNames.Creator, PdfString.Empty).CA();
        var baseState = ParseBaseState(await occ.GetOrNullAsync(KnownNames.BaseState).CA());
        var onArr = await ParseOnOffArray(occ, KnownNames.ON).CA();
        var offArr = await ParseOnOffArray(occ, KnownNames.OFF).CA();

        return new OptionalContentConfiguration(name, creator, baseState, onArr, offArr);
    }

    private static bool? ParseBaseState(PdfObject baseStateVal)
    {
        bool? baseState = baseStateVal.GetHashCode() switch
        {
            KnownNameKeys.ON => true,
            KnownNameKeys.OFF => false,
            _ => null
        };
        return baseState;
    }

    private static async ValueTask<PdfDictionary[]> ParseOnOffArray(PdfDictionary occ, PdfName dictName)
    {
        return await (await occ.GetOrDefaultAsync(dictName, PdfArray.Empty).CA())
            .AsAsync<PdfDictionary>().CA();
    }
}

public class OptionalContentState : IOptionalContentState
{
    private Dictionary<PdfDictionary, bool> groupStates;
    public IReadOnlyList<OptionalContentConfiguration> Configurations { get; }

    public OptionalContentState(Dictionary<PdfDictionary, bool> groupStates,
        IReadOnlyList<OptionalContentConfiguration> configurations)
    {
        this.groupStates = groupStates;
        Configurations = configurations;
    }

    public ValueTask<bool> IsGroupVisible(PdfDictionary? dictionary)
    {
        if (dictionary == null) return new(true);
        return new(groupStates.TryGetValue(dictionary, out var result) ? result : true);
    }

    public void ConfigureWith(OptionalContentConfiguration configuration) => 
        configuration.ApplyTo(groupStates);
}