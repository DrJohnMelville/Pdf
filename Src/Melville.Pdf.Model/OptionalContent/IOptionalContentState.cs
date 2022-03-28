using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public interface IOptionalContentState
{
    ValueTask<bool> IsGroupVisible(PdfDictionary? dictionary);
    IReadOnlyList<OptionalContentConfiguration> Configurations { get; }
    ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ConstructUiModel(PdfArray? order);
    event EventHandler<EventArgs>? SelectedContentChanged; 
    OptionalContentConfiguration? SelectedConfiguration { get; set; }
}