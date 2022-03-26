using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public record class OptionalContentConfiguration(
    PdfString Name,
    PdfString Creator,
    bool? BaseState,
    PdfDictionary[] On,
    PdfDictionary[]  Off
)
{
    public void ApplyTo(Dictionary<PdfDictionary, bool> groupStates)
    {
        if (BaseState.HasValue) SetValues(groupStates, groupStates.Keys.ToArray(), BaseState.Value);
        SetValues(groupStates, On, true);
        SetValues(groupStates, Off, false);
    }

    private void SetValues(Dictionary<PdfDictionary, bool> dict, IEnumerable<PdfDictionary> values, bool baseState)
    {
        foreach (var value in values)
        {
            dict[value] = baseState;
        }
    }
}