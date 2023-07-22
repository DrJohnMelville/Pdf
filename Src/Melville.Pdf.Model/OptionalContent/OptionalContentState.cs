using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.Model.OptionalContent;

internal partial class OptionalContentState : IOptionalContentState
{
    private Dictionary<PdfValueDictionary, OptionalGroup> groupStates;
    public IReadOnlyList<OptionalContentConfiguration> Configurations { get; }
    [AutoNotify] private OptionalContentConfiguration? selectedConfiguration;
    public event EventHandler<EventArgs>? SelectedContentChanged; 
    private bool allVisible = true;

    public OptionalContentState(Dictionary<PdfValueDictionary, OptionalGroup> groupStates,
        IReadOnlyList<OptionalContentConfiguration> configurations)
    {
        this.groupStates = groupStates;
        Configurations = configurations;
        RegisterGroupStateChangeNotifications(groupStates);
    }

    public bool AllVisible() => allVisible;
    private void RecomputeAllVisible()
    {
        allVisible = groupStates.Values.All(i => i.Visible);
    }

    private void RegisterGroupStateChangeNotifications(Dictionary<PdfValueDictionary, OptionalGroup> groupStates)
    {
        foreach (var state in groupStates.Values)
        {
            state.PropertyChanged += SignalSelectedContentChanged;
        }
    }
    private void SignalSelectedContentChanged(object? sender, EventArgs ea)
    {
        if (sender is OptionalGroup og && og.Visible)
        {
            selectedConfiguration?.HandleRadioButtonExclusivity(og);
        } 
        RecomputeAllVisible();
        SelectedContentChanged?.Invoke(this, EventArgs.Empty);
    }

    public async ValueTask<bool> IsGroupVisibleAsync(PdfValueDictionary? dictionary)
    {
        if (dictionary == null) return true;
        if ((await dictionary.GetOrDefaultAsync(
                KnownNames.TypeTName, KnownNames.DamagedRowsBeforeErrorTName).CA())
                .Equals(KnownNames.OCMDTName))
            return await
                new OptionalContentMemberDictionaryInterpreter(dictionary, this)
                    .ParseAsync().CA();
        return groupStates.TryGetValue(dictionary, out var result) ? result.Visible : true;
    }

    public void ConfigureWith(OptionalContentConfiguration configuration)
    {
        SelectedConfiguration = configuration;
        configuration.ApplyTo(groupStates);
    }


    public ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ConstructUiModelAsync(PdfValueArray? order) => 
        new UiModelParser(groupStates).ParseAsync(order);
}