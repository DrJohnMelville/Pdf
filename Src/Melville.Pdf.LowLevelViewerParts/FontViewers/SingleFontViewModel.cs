using System.ComponentModel.Design;
using Melville.Fonts;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class SingleFontViewModel(IGenericFont font)
{
    public IReadOnlyList<object> Tables { get; protected set; } = 
        [new GenericFontViewModel(font)];
}

public partial class GenericFontViewModel
{
    [FromConstructor] public IGenericFont Font { get; }
    public string Title => "Generic Font View";
    public string? ToolTip => null;
}