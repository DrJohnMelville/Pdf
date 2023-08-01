using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

[StaticSingleton]
internal sealed partial class AllOptionalContentVisible : IOptionalContentState
{
    public ValueTask<bool> IsGroupVisibleAsync(PdfValueDictionary? dictionary) => new(true);
    public event EventHandler<EventArgs>? SelectedContentChanged
    {
        add {}
        remove {}
    }
    public OptionalContentConfiguration? SelectedConfiguration { get; set; }
    public bool AllVisible() => true;

    public IReadOnlyList<OptionalContentConfiguration> Configurations =>
        Array.Empty<OptionalContentConfiguration>();
    public ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ConstructUiModelAsync(PdfValueArray? order) => 
        new(Array.Empty<IOptionalContentDisplayGroup>());
}