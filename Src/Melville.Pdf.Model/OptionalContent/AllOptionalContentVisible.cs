using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public sealed class AllOptionalContentVisible : IOptionalContentState
{
    public static readonly AllOptionalContentVisible Instance = new();
    private AllOptionalContentVisible() { }
    public ValueTask<bool> IsGroupVisible(PdfDictionary? dictionary) => new(true);
    public event EventHandler<EventArgs>? SelectedContentChanged
    {
        add {}
        remove {}
    }
    public OptionalContentConfiguration? SelectedConfiguration { get; set; }

    public IReadOnlyList<OptionalContentConfiguration> Configurations =>
        Array.Empty<OptionalContentConfiguration>();
    public ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ConstructUiModel(PdfArray? order) => 
        new(Array.Empty<IOptionalContentDisplayGroup>());
}