using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.Model.OptionalContent;

public partial class OptionalContentState : IOptionalContentState
{
    private Dictionary<PdfDictionary, OptionalGroup> groupStates;
    public IReadOnlyList<OptionalContentConfiguration> Configurations { get; }
    [AutoNotify] private OptionalContentConfiguration? selectedConfiguration;
    public bool HasOptionalGroups => Configurations.Count > 0;
    public bool HasMultipleConfigurations => Configurations.Count > 1;
    public event EventHandler<EventArgs>? SelectedContentChanged; 

    public OptionalContentState(Dictionary<PdfDictionary, OptionalGroup> groupStates,
        IReadOnlyList<OptionalContentConfiguration> configurations)
    {
        this.groupStates = groupStates;
        Configurations = configurations;
        RegisterGroupStateChangeNotifications(groupStates);
    }

    private void RegisterGroupStateChangeNotifications(Dictionary<PdfDictionary, OptionalGroup> groupStates)
    {
        foreach (var state in groupStates.Values)
        {
            state.PropertyChanged += SignalSelectedContentChanged;
        }
    }
    private void SignalSelectedContentChanged(object? sender, EventArgs ea) =>
        SignalSelectedContentChanged();
    private void SignalSelectedContentChanged()
    {
        SelectedContentChanged?.Invoke(this, EventArgs.Empty);
    }

    public ValueTask<bool> IsGroupVisible(PdfDictionary? dictionary)
    {
        if (dictionary == null) return new(true);
        return new(groupStates.TryGetValue(dictionary, out var result) ? result.Visible : true);
    }

    public void ConfigureWith(OptionalContentConfiguration configuration)
    {
        SelectedConfiguration = configuration;
        configuration.ApplyTo(groupStates);
    }


    public ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ConstructUiModel(PdfArray order) => 
        new UiModelParser(groupStates).Parse(order);
}