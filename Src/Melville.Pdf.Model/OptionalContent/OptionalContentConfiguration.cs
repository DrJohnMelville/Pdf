using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public record class OptionalContentConfiguration(
    string Name,
    string Creator,
    bool? BaseState,
    PdfDictionary[] On,
    PdfDictionary[]  Off,
    PdfArray Order
)
{
    public void ApplyTo(IReadOnlyDictionary<PdfDictionary, OptionalGroup> groupStates)
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
}