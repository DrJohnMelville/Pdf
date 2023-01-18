using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

/// <summary>
/// Represents a PDF? Optional Content Configuration. spec 2.0 sec 8.11.4.3
/// </summary>
/// <param name="Name">Name of the group, suitable for display to the user</param>
/// <param name="Creator">Application or feature that created the group</param>
/// <param name="BaseState">Default visibility state of groups in this configuration</param>
/// <param name="On">Groups to be turned on at initialization.</param>
/// <param name="Off">Groups to be turned off at initialization.</param>
/// <param name="Order">Specifies the order in which groups are presented in the user interface.</param>
/// <param name="RadioButtons">Defines mutually exclusive visibility between content groups.</param>
public record class OptionalContentConfiguration(
    string Name,
    string Creator,
    bool? BaseState,
    PdfDictionary[] On,
    PdfDictionary[]  Off,
    PdfArray Order,
    IReadOnlyList<OptionalContentExclusionGroup> RadioButtons 
)

{
    internal void ApplyTo(IReadOnlyDictionary<PdfDictionary, OptionalGroup> groupStates)
    {
        if (BaseState.HasValue) SetValues(groupStates, groupStates.Keys.ToArray(), BaseState.Value);
        SetValues(groupStates, On, true);
        SetValues(groupStates, Off, false);
    }

    private void SetValues(IReadOnlyDictionary<PdfDictionary, OptionalGroup> dict, IEnumerable<PdfDictionary> values,
        bool baseState)
    {
        foreach (var value in values)
        {
            if (dict.TryGetValue(value, out var optionalGroup)) optionalGroup.Visible = baseState;
        }

    }

    
    internal void HandleRadioButtonExclusivity(OptionalGroup og)
    {
        foreach (var radioButtonGroup in RadioButtons)
        {
            radioButtonGroup.HandleRadioButtonExclusivity(og);
        }
    }
}

/// <summary>
/// Defines a set of OptionGroup items that cannot be simultaneously displayed
/// </summary>
public readonly struct OptionalContentExclusionGroup
{
    private readonly IReadOnlyList<OptionalGroup> items;

    internal OptionalContentExclusionGroup(IReadOnlyList<OptionalGroup> items)
    {
        this.items = items;
    }

    internal void HandleRadioButtonExclusivity(OptionalGroup og)
    {
        if (!items.Contains(og)) return;
        foreach (var item in items)
        {
            if (item.Visible && item != og) item.Visible = false;
        }
    }
}